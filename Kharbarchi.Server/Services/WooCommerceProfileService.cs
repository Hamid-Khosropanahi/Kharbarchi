using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Kharbarchi.Server.Data;
using Kharbarchi.Server.Infrastructure.Safety;
using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Contracts.WooCommerce;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Services;

public sealed class WooCommerceProfileService
{
    private readonly AppDbContext _context;
    private readonly IDataProtector _protector;
    private readonly EnvironmentSafetyGuard _guard;

    public WooCommerceProfileService(AppDbContext context, IDataProtectionProvider protectionProvider, EnvironmentSafetyGuard guard)
    {
        _context = context;
        _protector = protectionProvider.CreateProtector("Kharbarchi.WooCommerce.ConnectionProfile.v1");
        _guard = guard;
    }

    public async Task<IReadOnlyList<WooConnectionProfileDto>> GetProfilesAsync(CancellationToken cancellationToken)
    {
        var profiles = await _context.WooCommerceConnectionProfiles
            .AsNoTracking()
            .OrderBy(x => x.EnvironmentType)
            .ThenByDescending(x => x.IsActive)
            .ThenBy(x => x.ProfileName)
            .ToListAsync(cancellationToken);
        return profiles.Select(ToDto).ToList();
    }

    public async Task<WooConnectionProfileDto> SaveAsync(WooConnectionProfileUpsertRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var environmentType = NormalizeEnvironment(request.EnvironmentType);
        var now = DateTime.UtcNow;
        WooCommerceConnectionProfile profile;

        if (request.Id is > 0)
        {
            profile = await _context.WooCommerceConnectionProfiles
                .SingleOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken)
                ?? throw new KeyNotFoundException("WooCommerce connection profile was not found.");
        }
        else
        {
            profile = new WooCommerceConnectionProfile
            {
                CreatedAtUtc = now
            };
            _context.WooCommerceConnectionProfiles.Add(profile);
        }

        profile.ProfileName = request.ProfileName.Trim();
        profile.EnvironmentType = environmentType;
        profile.BaseUrl = request.BaseUrl.Trim().TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(request.ConsumerKey))
        {
            profile.ConsumerKey = request.ConsumerKey.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.ConsumerSecret))
        {
            profile.ProtectedConsumerSecret = _protector.Protect(request.ConsumerSecret.Trim());
        }

        if (string.IsNullOrWhiteSpace(profile.ConsumerKey) || string.IsNullOrWhiteSpace(profile.ProtectedConsumerSecret))
        {
            throw new InvalidOperationException("Consumer Key and Consumer Secret are required.");
        }

        profile.ApiVersion = NormalizeApiVersion(request.ApiVersion);
        profile.VerifySsl = environmentType == "Production" || request.VerifySsl;
        profile.TimeoutSeconds = Math.Clamp(request.TimeoutSeconds, 5, 180);
        profile.IsActive = request.IsActive;
        profile.UpdatedAtUtc = now;

        if (profile.IsActive)
        {
            var otherActive = await _context.WooCommerceConnectionProfiles
                .Where(x => x.Id != profile.Id && x.EnvironmentType == environmentType && x.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var other in otherActive)
            {
                other.IsActive = false;
                other.UpdatedAtUtc = now;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(profile);
    }

    public async Task<WooProfileConnection> GetConnectionAsync(int profileId, CancellationToken cancellationToken)
    {
        var profile = await _context.WooCommerceConnectionProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == profileId, cancellationToken)
            ?? throw new KeyNotFoundException("WooCommerce connection profile was not found.");
        var secret = _protector.Unprotect(profile.ProtectedConsumerSecret);

        // Validate profile against environment safety guard before returning a live connection
        try
        {
            _guard.ValidateWooProfile(profile.BaseUrl, profile.VerifySsl, profile.EnvironmentType, profile.Id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("WooCommerce profile is not allowed by environment safety policies.", ex);
        }
        return new WooProfileConnection(
            profile.Id,
            profile.ProfileName,
            profile.EnvironmentType,
            profile.BaseUrl,
            profile.ConsumerKey,
            secret,
            profile.ApiVersion,
            profile.EnvironmentType == "Production" || profile.VerifySsl,
            profile.TimeoutSeconds);
    }

    public async Task RecordTestAsync(int profileId, WooConnectionProfileTestResultDto result, CancellationToken cancellationToken)
    {
        var profile = await _context.WooCommerceConnectionProfiles
            .SingleAsync(x => x.Id == profileId, cancellationToken);
        profile.LastTestedAtUtc = result.TestedAtUtc;
        profile.LastTestSucceeded = result.Success;
        profile.LastTestMessage = Trim(result.Message, 2000);
        profile.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static WooConnectionProfileDto ToDto(WooCommerceConnectionProfile profile) => new()
    {
        Id = profile.Id,
        ProfileName = profile.ProfileName,
        EnvironmentType = profile.EnvironmentType,
        BaseUrl = profile.BaseUrl,
        ConsumerKeyMasked = Mask(profile.ConsumerKey),
        HasConsumerSecret = !string.IsNullOrWhiteSpace(profile.ProtectedConsumerSecret),
        ApiVersion = profile.ApiVersion,
        VerifySsl = profile.VerifySsl,
        TimeoutSeconds = profile.TimeoutSeconds,
        IsActive = profile.IsActive,
        LastTestedAtUtc = profile.LastTestedAtUtc,
        LastTestSucceeded = profile.LastTestSucceeded,
        LastTestMessage = profile.LastTestMessage,
        CreatedAtUtc = profile.CreatedAtUtc,
        UpdatedAtUtc = profile.UpdatedAtUtc
    };

    private static void Validate(WooConnectionProfileUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProfileName))
        {
            throw new ArgumentException("Profile name is required.");
        }

        if (!Uri.TryCreate(request.BaseUrl?.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new ArgumentException("BaseUrl must be an absolute HTTP or HTTPS URL.");
        }

        if (NormalizeEnvironment(request.EnvironmentType) == "Production" && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Production WooCommerce profiles require HTTPS.");
        }
    }

    private static string NormalizeEnvironment(string? value) =>
        string.Equals(value?.Trim(), "Production", StringComparison.OrdinalIgnoreCase) ? "Production" : "Local";

    private static string NormalizeApiVersion(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "wc/v3" : value.Trim().Trim('/');
        return normalized.StartsWith("wc/", StringComparison.OrdinalIgnoreCase) ? normalized : "wc/v3";
    }

    private static string Mask(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= 8 ? "********" : $"{value[..3]}…{value[^4..]}";
    }

    private static string Trim(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}

public sealed record WooProfileConnection(
    int Id,
    string ProfileName,
    string EnvironmentType,
    string BaseUrl,
    string ConsumerKey,
    string ConsumerSecret,
    string ApiVersion,
    bool VerifySsl,
    int TimeoutSeconds);

public sealed class ProfileWooCommerceClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly WooProfileConnection _connection;

    public ProfileWooCommerceClient(WooProfileConnection connection)
    {
        _connection = connection;
        var handler = new HttpClientHandler();
        if (!connection.VerifySsl && connection.EnvironmentType == "Local")
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(connection.BaseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(Math.Clamp(connection.TimeoutSeconds, 5, 180))
        };
        var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{connection.ConsumerKey}:{connection.ConsumerSecret}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Kharbarchi-ERP/1.0");
    }

    public string BuildSafeUrl(string relativeUrl) =>
        new Uri(_httpClient.BaseAddress!, relativeUrl.TrimStart('/')).ToString();

    public async Task<WooConnectionProfileTestResultDto> TestAsync(CancellationToken cancellationToken)
    {
        var relativeUrl = $"wp-json/{_connection.ApiVersion}/products?per_page=1";
        var watch = Stopwatch.StartNew();
        try
        {
            using var response = await SendAsync(HttpMethod.Get, relativeUrl, null, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            watch.Stop();
            return new WooConnectionProfileTestResultDto
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                Url = BuildSafeUrl(relativeUrl),
                Message = response.IsSuccessStatusCode
                    ? "اتصال WooCommerce با موفقیت آزمایش شد."
                    : $"WooCommerce پاسخ خطا داد: {(int)response.StatusCode} {response.ReasonPhrase}",
                ElapsedMilliseconds = watch.ElapsedMilliseconds,
                ResponsePreview = Trim(body, 3000),
                TestedAtUtc = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            watch.Stop();
            return new WooConnectionProfileTestResultDto
            {
                Success = false,
                Url = BuildSafeUrl(relativeUrl),
                Message = ex.Message,
                ElapsedMilliseconds = watch.ElapsedMilliseconds,
                ResponsePreview = Trim(ex.GetType().Name, 3000),
                TestedAtUtc = DateTime.UtcNow
            };
        }
    }

    public async Task<JsonDocument> SendJsonAsync(
        HttpMethod method,
        string relativeUrl,
        string? json,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(method, relativeUrl, json, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new WooCommerceProfileException(
                $"WooCommerce HTTP {(int)response.StatusCode}: {Trim(body, 3000)}",
                (int)response.StatusCode,
                BuildSafeUrl(relativeUrl),
                Trim(body, 3000));
        }

        return JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
    }

    public async Task<JsonDocument?> TryGetJsonAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Get, relativeUrl, null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativeUrl,
        string? json,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(method, relativeUrl.TrimStart('/'));
                if (json is not null)
                {
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (attempt >= 3 || !IsTransient(response.StatusCode))
                {
                    return response;
                }

                response.Dispose();
            }
            catch (HttpRequestException) when (attempt < 3)
            {
                // Limited retry for a transient connection failure.
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < 3)
            {
                // Limited retry for a transport timeout.
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
        }
    }

    private static bool IsTransient(System.Net.HttpStatusCode statusCode) =>
        statusCode is System.Net.HttpStatusCode.RequestTimeout
            or System.Net.HttpStatusCode.TooManyRequests
            or System.Net.HttpStatusCode.BadGateway
            or System.Net.HttpStatusCode.ServiceUnavailable
            or System.Net.HttpStatusCode.GatewayTimeout
        || (int)statusCode >= 500;

    private static string Trim(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    public void Dispose() => _httpClient.Dispose();
}

public sealed class WooCommerceProfileException : Exception
{
    public WooCommerceProfileException(string message, int responseCode, string requestUrl, string responseSummary)
        : base(message)
    {
        ResponseCode = responseCode;
        RequestUrl = requestUrl;
        ResponseSummary = responseSummary;
    }

    public int ResponseCode { get; }
    public string RequestUrl { get; }
    public string ResponseSummary { get; }
}
