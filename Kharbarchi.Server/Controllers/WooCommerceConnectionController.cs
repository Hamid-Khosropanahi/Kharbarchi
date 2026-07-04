using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Kharbarchi.Server.Infrastructure.Safety;
using Kharbarchi.Server.Models;
using Kharbarchi.Server.Options;
using Kharbarchi.Server.Services;
using Kharbarchi.Shared.Contracts.WooCommerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/admin/woocommerce-connection")]
[Authorize(Roles = "SuperAdmin,Admin,CentralSyncAgent")]
public sealed class WooCommerceConnectionController : ControllerBase
{
    private readonly WooCommerceRuntimeSettingsStore _settingsStore;
    private readonly ILogger<WooCommerceConnectionController> _logger;
    private readonly EnvironmentSafetyGuard _guard;
    private readonly WooCommerceOptions _wooOptions;

    public WooCommerceConnectionController(
        WooCommerceRuntimeSettingsStore settingsStore,
        ILogger<WooCommerceConnectionController> logger,
        EnvironmentSafetyGuard guard,
        IOptions<WooCommerceOptions> wooOptions)
    {
        _settingsStore = settingsStore;
        _logger = logger;
        _guard = guard;
        _wooOptions = wooOptions.Value;
    }

    [HttpGet("settings")]
    public async Task<ActionResult<WooConnectionSettingsDto>> GetSettings(CancellationToken cancellationToken)
    {
        var settings = await _settingsStore.LoadAsync(cancellationToken);
        return Ok(_settingsStore.ToDto(settings));
    }

    [HttpPut("settings")]
    public async Task<ActionResult<WooConnectionSettingsDto>> SaveSettings([FromBody] WooConnectionSettingsDto request, CancellationToken cancellationToken)
    {
        if (!IsValidBaseUrl(request.BaseUrl, out var message))
        {
            return BadRequest(new { message });
        }

        if (string.IsNullOrWhiteSpace(request.ConsumerKey))
        {
            return BadRequest(new { message = "Consumer Key نباید خالی باشد." });
        }

        ValidateWooTarget(
            request.BaseUrl,
            request.AllowInsecureLocalhostSsl);
        var settings = await _settingsStore.SaveAsync(request, cancellationToken);
        return Ok(_settingsStore.ToDto(settings));
    }

    [HttpPost("test")]
    public async Task<ActionResult<WooApiTestResultDto>> Test([FromBody] WooConnectionTestRequest request, CancellationToken cancellationToken)
    {
        var settings = await _settingsStore.MergeAsync(request, cancellationToken);
        if (!IsValidBaseUrl(settings.BaseUrl, out var message))
        {
            return BadRequest(new WooApiTestResultDto { Success = false, Message = message });
        }

        var relativeUrl = request.TestKind?.Trim().ToLowerInvariant() switch
        {
            "wordpress" or "wp" => "/wp-json/",
            "system" or "status" => "/wp-json/wc/v3/system_status",
            _ => "/wp-json/wc/v3/products?per_page=1"
        };

        var rawRequest = new WooRawApiRequest
        {
            Method = "GET",
            RelativeUrl = relativeUrl,
            BaseUrl = settings.BaseUrl,
            ConsumerKey = settings.ConsumerKey,
            ConsumerSecret = settings.ConsumerSecret,
            TimeoutSeconds = settings.TimeoutSeconds,
            AllowInsecureLocalhostSsl = settings.AllowInsecureLocalhostSsl
        };

        return Ok(await ExecuteAsync(rawRequest, settings, cancellationToken));
    }

    [HttpPost("raw")]
    public async Task<ActionResult<WooApiTestResultDto>> Raw([FromBody] WooRawApiRequest request, CancellationToken cancellationToken)
    {
        var settings = await _settingsStore.MergeAsync(request, cancellationToken);
        if (!IsValidBaseUrl(settings.BaseUrl, out var message))
        {
            return BadRequest(new WooApiTestResultDto { Success = false, Message = message });
        }

        return Ok(await ExecuteAsync(request, settings, cancellationToken));
    }

    private async Task<WooApiTestResultDto> ExecuteAsync(WooRawApiRequest request, WooCommerceRuntimeSettings settings, CancellationToken cancellationToken)
    {
        ValidateWooTarget(settings.BaseUrl, settings.AllowInsecureLocalhostSsl);
        var method = NormalizeMethod(request.Method);
        var relativeUrl = NormalizeRelativeUrl(request.RelativeUrl);
        var fullUrl = BuildFullUrl(settings.BaseUrl, relativeUrl);
        var watch = Stopwatch.StartNew();

        try
        {
            using var httpClient = CreateHttpClient(settings);
            using var httpRequest = new HttpRequestMessage(method, fullUrl);

            if (RequiresWooAuth(relativeUrl))
            {
                ApplyBasicAuth(httpRequest, settings);
            }

            if (method != HttpMethod.Get && method != HttpMethod.Head && !string.IsNullOrWhiteSpace(request.BodyJson))
            {
                httpRequest.Content = new StringContent(request.BodyJson!, Encoding.UTF8, "application/json");
            }

            using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            watch.Stop();

            return new WooApiTestResultDto
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                Method = method.Method,
                Url = fullUrl,
                ElapsedMilliseconds = watch.ElapsedMilliseconds,
                Message = response.IsSuccessStatusCode
                    ? "اتصال موفق بود. پاسخ API دریافت شد."
                    : $"API پاسخ خطا داد: {(int)response.StatusCode} {response.ReasonPhrase}",
                ResponseBody = PrettyOrTrim(body),
                CurlPreview = BuildCurlPreview(method, fullUrl, request.BodyJson, RequiresWooAuth(relativeUrl))
            };
        }
        catch (Exception ex)
        {
            watch.Stop();
            var safeEndpoint = ToSafeEndpointForLog(fullUrl);
            _logger.LogError(ex, "WooCommerce API test failed. Endpoint={Endpoint}", safeEndpoint);

            return new WooApiTestResultDto
            {
                Success = false,
                Method = method.Method,
                Url = fullUrl,
                ElapsedMilliseconds = watch.ElapsedMilliseconds,
                Message = ex.Message,
                ResponseBody = ex.ToString(),
                CurlPreview = BuildCurlPreview(method, fullUrl, request.BodyJson, RequiresWooAuth(relativeUrl))
            };
        }

    }

    private void ValidateWooTarget(string baseUrl, bool allowInsecureLocalhostSsl)
    {
        _guard.ValidateWooProfile(
            baseUrl,
            !allowInsecureLocalhostSsl,
            _wooOptions.EnvironmentType,
            expectedProductionBaseUrl: _wooOptions.BaseUrl);
    }

    private static string ToSafeEndpointForLog(string? fullUrl)
    {
        if (string.IsNullOrWhiteSpace(fullUrl)) return "[invalid-url]";

        if (!Uri.TryCreate(fullUrl.Trim(), UriKind.Absolute, out var uri))
        {
            return "[invalid-url]";
        }

        var scheme = uri.Scheme;
        var host = uri.Host;
        var port = uri.IsDefaultPort ? string.Empty : ":" + uri.Port.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return $"{scheme}://{host}{port}";
    }
    private static HttpClient CreateHttpClient(WooCommerceRuntimeSettings settings)
    {
        var handler = new HttpClientHandler();
        if (settings.AllowInsecureLocalhostSsl && IsLocalhost(settings.BaseUrl))
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
        };
    }

    private static void ApplyBasicAuth(HttpRequestMessage request, WooCommerceRuntimeSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ConsumerKey) || string.IsNullOrWhiteSpace(settings.ConsumerSecret))
        {
            return;
        }

        var bytes = Encoding.ASCII.GetBytes($"{settings.ConsumerKey}:{settings.ConsumerSecret}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
    }

    private static bool RequiresWooAuth(string relativeUrl)
    {
        return relativeUrl.Contains("/wp-json/wc/", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeRelativeUrl(string? value)
    {
        var relativeUrl = string.IsNullOrWhiteSpace(value) ? "/wp-json/wc/v3/products?per_page=1" : value.Trim();
        if (Uri.TryCreate(relativeUrl, UriKind.Absolute, out var absolute))
        {
            return absolute.PathAndQuery;
        }

        return relativeUrl.StartsWith('/') ? relativeUrl : "/" + relativeUrl;
    }

    private static string BuildFullUrl(string baseUrl, string relativeUrl)
    {
        return baseUrl.TrimEnd('/') + relativeUrl;
    }

    private static HttpMethod NormalizeMethod(string? method)
    {
        return (method ?? "GET").Trim().ToUpperInvariant() switch
        {
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "PATCH" => HttpMethod.Patch,
            "DELETE" => HttpMethod.Delete,
            "HEAD" => HttpMethod.Head,
            _ => HttpMethod.Get
        };
    }

    private static bool IsValidBaseUrl(string? value, out string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            message = "آدرس سایت ووکامرس خالی است.";
            return false;
        }

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
        {
            message = "آدرس سایت ووکامرس معتبر نیست.";
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
        {
            message = "آدرس سایت باید با http یا https شروع شود.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private static bool IsLocalhost(string baseUrl)
    {
        return Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            && (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase));
    }

    private static string PrettyOrTrim(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        return body.Length > 20_000 ? body[..20_000] + "\n... response truncated ..." : body;
    }

    private static string BuildCurlPreview(HttpMethod method, string fullUrl, string? bodyJson, bool authenticated)
    {
        var builder = new StringBuilder();
        builder.Append("curl -X ").Append(method.Method).Append(' ').Append('"').Append(fullUrl).Append('"');
        if (authenticated)
        {
            builder.Append(" \\\n  -u \"ck_xxx:cs_xxx\"");
        }

        if (method != HttpMethod.Get && !string.IsNullOrWhiteSpace(bodyJson))
        {
            builder.Append(" \\\n  -H \"Content-Type: application/json\" \\\n  -d '").Append(bodyJson.Replace("'", "'\\''")).Append("'");
        }

        return builder.ToString();
    }
}
