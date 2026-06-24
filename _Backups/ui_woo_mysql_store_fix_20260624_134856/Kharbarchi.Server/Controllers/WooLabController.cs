using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Kharbarchi.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdmin,Admin,CentralSyncAgent")]
[Route("api/admin/woo-lab")]
public sealed class WooLabController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<WooLabController> _logger;
    private readonly IConfiguration _configuration;

    public WooLabController(AppDbContext db, ILogger<WooLabController> logger, IConfiguration configuration)
    {
        _db = db;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new
        {
            ok = true,
            message = "WooCommerce lab endpoint is reachable.",
            serverTimeUtc = DateTimeOffset.UtcNow,
            configured = GetConfigurationState()
        });
    }

    [HttpGet("config-state")]
    public IActionResult ConfigState()
    {
        return Ok(GetConfigurationState());
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test([FromBody] WooLabRequest request, CancellationToken cancellationToken)
    {
        var prepared = PrepareRequest(request, requireCredentials: false);
        if (prepared.Error is not null) return BadRequest(prepared.Error);

        var resolved = prepared.Request!;
        var endpoint = string.IsNullOrWhiteSpace(resolved.Endpoint) ? "wp-json/" : resolved.Endpoint.TrimStart('/');
        var result = await FetchFromWooAsync(resolved, endpoint, cancellationToken);

        return Ok(new
        {
            ok = result.IsSuccessStatusCode,
            statusCode = (int)result.StatusCode,
            reasonPhrase = result.ReasonPhrase,
            url = HideCredentials(result.Url),
            elapsedMs = result.ElapsedMilliseconds,
            usedCredentials = HasCredentials(resolved),
            credentialSource = prepared.CredentialSource,
            body = TryParseJson(result.Body)
        });
    }

    [HttpPost("import-default")]
    public async Task<IActionResult> ImportDefault([FromBody] WooLabRequest request, CancellationToken cancellationToken)
    {
        var prepared = PrepareRequest(request, requireCredentials: true);
        if (prepared.Error is not null) return BadRequest(prepared.Error);

        var resolved = prepared.Request!;
        await EnsureImportTableAsync(cancellationToken);

        var sources = new List<(string Type, string Endpoint)>
        {
            ("wp-root", "wp-json/"),
            ("category", "wp-json/wc/v3/products/categories?per_page=100"),
            ("product", $"wp-json/wc/v3/products?per_page={Clamp(resolved.ProductTake, 1, 100)}"),
            ("order", $"wp-json/wc/v3/orders?per_page={Clamp(resolved.OrderTake, 1, 100)}")
        };

        var report = new List<object>();
        var imported = 0;

        foreach (var source in sources)
        {
            var fetch = await FetchFromWooAsync(resolved, source.Endpoint, cancellationToken);
            await InsertRawRecordAsync(source.Type, HideCredentials(fetch.Url), (int)fetch.StatusCode, fetch.Body, cancellationToken);
            imported++;

            report.Add(new
            {
                source.Type,
                source.Endpoint,
                url = HideCredentials(fetch.Url),
                statusCode = (int)fetch.StatusCode,
                fetch.ReasonPhrase,
                fetch.ElapsedMilliseconds,
                saved = true,
                sample = fetch.Body.Length > 900 ? fetch.Body[..900] + "..." : fetch.Body
            });
        }

        return Ok(new
        {
            ok = true,
            importedRecords = imported,
            databaseTable = "khb_imported_woocommerce_records",
            credentialSource = prepared.CredentialSource,
            message = "Raw WooCommerce responses saved to MySQL. Next step is mapping raw records to product/order tables.",
            report
        });
    }

    private PreparedWooRequest PrepareRequest(WooLabRequest? request, bool requireCredentials)
    {
        request ??= new WooLabRequest();

        var configuredBaseUrl = _configuration["WooCommerce:BaseUrl"];
        var configuredKey = _configuration["WooCommerce:ConsumerKey"];
        var configuredSecret = _configuration["WooCommerce:ConsumerSecret"];
        var configuredTimeoutText = _configuration["WooCommerce:TimeoutSeconds"];

        var resolved = new WooLabRequest
        {
            BaseUrl = FirstNonEmpty(request.BaseUrl, configuredBaseUrl),
            ConsumerKey = FirstNonEmpty(request.ConsumerKey, configuredKey),
            ConsumerSecret = FirstNonEmpty(request.ConsumerSecret, configuredSecret),
            Endpoint = string.IsNullOrWhiteSpace(request.Endpoint) ? "wp-json/" : request.Endpoint,
            ProductTake = request.ProductTake <= 0 ? 50 : request.ProductTake,
            OrderTake = request.OrderTake <= 0 ? 50 : request.OrderTake,
            TimeoutSeconds = request.TimeoutSeconds <= 0 && int.TryParse(configuredTimeoutText, out var timeout) ? timeout : request.TimeoutSeconds
        };

        if (string.IsNullOrWhiteSpace(resolved.BaseUrl) || !Uri.TryCreate(resolved.BaseUrl, UriKind.Absolute, out var uri))
        {
            return PreparedWooRequest.Fail(new
            {
                error = "BaseUrl must be a valid absolute URL.",
                hint = "Set WooCommerce:BaseUrl in appsettings.Development.json/User Secrets or fill BaseUrl in the page. Example: https://localhost:4433/Kharbarchi"
            });
        }

        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            return PreparedWooRequest.Fail(new { error = "BaseUrl must start with http or https." });

        if (requireCredentials && !HasCredentials(resolved))
        {
            return PreparedWooRequest.Fail(new
            {
                error = "ConsumerKey and ConsumerSecret are required for WooCommerce import.",
                hint = "The test can pass without credentials when it only checks WordPress/root endpoints. Import uses wc/v3 products/orders and must have ck/cs. Enter them on the import page or set WooCommerce:ConsumerKey and WooCommerce:ConsumerSecret in User Secrets/appsettings.Development.json."
            });
        }

        var credentialSource = "none";
        if (!string.IsNullOrWhiteSpace(request.ConsumerKey) && !string.IsNullOrWhiteSpace(request.ConsumerSecret))
            credentialSource = "page request";
        else if (!string.IsNullOrWhiteSpace(configuredKey) && !string.IsNullOrWhiteSpace(configuredSecret))
            credentialSource = "server configuration";

        return PreparedWooRequest.Ok(resolved, credentialSource);
    }

    private object GetConfigurationState()
    {
        var baseUrl = _configuration["WooCommerce:BaseUrl"];
        var key = _configuration["WooCommerce:ConsumerKey"];
        var secret = _configuration["WooCommerce:ConsumerSecret"];
        return new
        {
            hasBaseUrl = !string.IsNullOrWhiteSpace(baseUrl),
            baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl,
            hasConsumerKey = !string.IsNullOrWhiteSpace(key),
            hasConsumerSecret = !string.IsNullOrWhiteSpace(secret),
            note = "ConsumerKey/ConsumerSecret are intentionally not returned."
        };
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
        }
        return string.Empty;
    }

    private static bool HasCredentials(WooLabRequest request)
        => !string.IsNullOrWhiteSpace(request.ConsumerKey) && !string.IsNullOrWhiteSpace(request.ConsumerSecret);

    private static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);

    private async Task<WooFetchResult> FetchFromWooAsync(WooLabRequest request, string endpoint, CancellationToken cancellationToken)
    {
        var baseUri = new Uri(request.BaseUrl.TrimEnd('/') + "/");
        var fullUri = new Uri(baseUri, endpoint.TrimStart('/'));
        fullUri = AddWooCredentials(fullUri, request.ConsumerKey, request.ConsumerSecret);

        using var handler = new HttpClientHandler();
        if (IsLocalhost(baseUri))
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        using var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(request.TimeoutSeconds <= 0 ? 60 : request.TimeoutSeconds)
        };

        var started = DateTimeOffset.UtcNow;
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, fullUri);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            using var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new WooFetchResult(fullUri.ToString(), response.StatusCode, response.ReasonPhrase ?? string.Empty, body, (long)(DateTimeOffset.UtcNow - started).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WooCommerce fetch failed for {Url}", HideCredentials(fullUri.ToString()));
            return new WooFetchResult(fullUri.ToString(), HttpStatusCode.ServiceUnavailable, ex.GetType().Name, ex.ToString(), (long)(DateTimeOffset.UtcNow - started).TotalMilliseconds);
        }
    }

    private static Uri AddWooCredentials(Uri uri, string? key, string? secret)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
            return uri;

        var separator = string.IsNullOrEmpty(uri.Query) ? "?" : "&";
        var url = uri + separator + "consumer_key=" + Uri.EscapeDataString(key) + "&consumer_secret=" + Uri.EscapeDataString(secret);
        return new Uri(url);
    }

    private static string HideCredentials(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        return System.Text.RegularExpressions.Regex.Replace(url, "(consumer_key|consumer_secret)=([^&]+)", "$1=***", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static bool IsLocalhost(Uri uri)
        => uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
           || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
           || uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase);

    private async Task EnsureImportTableAsync(CancellationToken cancellationToken)
    {
        await _db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS `khb_imported_woocommerce_records` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `SourceType` varchar(64) NOT NULL,
    `SourceUrl` varchar(2048) NOT NULL,
    `HttpStatusCode` int NOT NULL,
    `RawJson` longtext NOT NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_khb_imported_woocommerce_records_SourceType` (`SourceType`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
""", cancellationToken);
    }

    private async Task InsertRawRecordAsync(string sourceType, string sourceUrl, int statusCode, string rawJson, CancellationToken cancellationToken)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
INSERT INTO `khb_imported_woocommerce_records`
(`SourceType`, `SourceUrl`, `HttpStatusCode`, `RawJson`, `CreatedAtUtc`)
VALUES ({sourceType}, {sourceUrl}, {statusCode}, {rawJson ?? string.Empty}, {DateTime.UtcNow});
""", cancellationToken);
    }

    private static object? TryParseJson(string text)
    {
        try
        {
            using var document = JsonDocument.Parse(text);
            return JsonSerializer.Deserialize<object>(document.RootElement.GetRawText());
        }
        catch
        {
            return text;
        }
    }

    private sealed record WooFetchResult(string Url, HttpStatusCode StatusCode, string ReasonPhrase, string Body, long ElapsedMilliseconds)
    {
        public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode <= 299;
    }

    private sealed record PreparedWooRequest(WooLabRequest? Request, object? Error, string CredentialSource)
    {
        public static PreparedWooRequest Ok(WooLabRequest request, string credentialSource) => new(request, null, credentialSource);
        public static PreparedWooRequest Fail(object error) => new(null, error, "none");
    }

    public sealed class WooLabRequest
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ConsumerKey { get; set; } = string.Empty;
        public string ConsumerSecret { get; set; } = string.Empty;
        public string Endpoint { get; set; } = "wp-json/";
        public int ProductTake { get; set; } = 50;
        public int OrderTake { get; set; } = 50;
        public int TimeoutSeconds { get; set; } = 60;
    }
}
