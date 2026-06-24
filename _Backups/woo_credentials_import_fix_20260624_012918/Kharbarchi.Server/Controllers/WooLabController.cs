using System.Net;
using System.Net.Http.Headers;
using System.Text;
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

    public WooLabController(AppDbContext db, ILogger<WooLabController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new
        {
            ok = true,
            message = "WooCommerce lab endpoint is reachable.",
            serverTimeUtc = DateTimeOffset.UtcNow
        });
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test([FromBody] WooLabRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidateRequest(request, requireCredentials: false);
        if (validation is not null) return BadRequest(validation);

        var endpoint = string.IsNullOrWhiteSpace(request.Endpoint) ? "wp-json/" : request.Endpoint.TrimStart('/');
        var result = await FetchFromWooAsync(request, endpoint, cancellationToken);

        return Ok(new
        {
            ok = result.IsSuccessStatusCode,
            statusCode = (int)result.StatusCode,
            reasonPhrase = result.ReasonPhrase,
            url = result.Url,
            elapsedMs = result.ElapsedMilliseconds,
            body = TryParseJson(result.Body)
        });
    }

    [HttpPost("import-default")]
    public async Task<IActionResult> ImportDefault([FromBody] WooLabRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidateRequest(request, requireCredentials: true);
        if (validation is not null) return BadRequest(validation);

        await EnsureImportTableAsync(cancellationToken);

        var sources = new List<(string Type, string Endpoint)>
        {
            ("wp-root", "wp-json/"),
            ("category", $"wp-json/wc/v3/products/categories?per_page=100"),
            ("product", $"wp-json/wc/v3/products?per_page={Clamp(request.ProductTake, 1, 100)}"),
            ("order", $"wp-json/wc/v3/orders?per_page={Clamp(request.OrderTake, 1, 100)}")
        };

        var report = new List<object>();
        var imported = 0;

        foreach (var source in sources)
        {
            var fetch = await FetchFromWooAsync(request, source.Endpoint, cancellationToken);
            report.Add(new
            {
                source.Type,
                source.Endpoint,
                fetch.Url,
                fetch.StatusCode,
                fetch.ReasonPhrase,
                fetch.ElapsedMilliseconds,
                sample = fetch.Body.Length > 900 ? fetch.Body[..900] + "..." : fetch.Body
            });

            await InsertRawRecordAsync(source.Type, fetch.Url, (int)fetch.StatusCode, fetch.Body, cancellationToken);
            imported++;
        }

        return Ok(new
        {
            ok = true,
            importedRecords = imported,
            databaseTable = "khb_imported_woocommerce_records",
            message = "Raw WooCommerce responses saved to MySQL. Next step is mapping raw records to product/order tables.",
            report
        });
    }

    private static object? ValidateRequest(WooLabRequest request, bool requireCredentials)
    {
        if (request is null)
            return new { error = "Request body is required." };

        if (string.IsNullOrWhiteSpace(request.BaseUrl) || !Uri.TryCreate(request.BaseUrl, UriKind.Absolute, out var uri))
            return new { error = "BaseUrl must be a valid absolute URL." };

        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            return new { error = "BaseUrl must start with http or https." };

        if (requireCredentials && (string.IsNullOrWhiteSpace(request.ConsumerKey) || string.IsNullOrWhiteSpace(request.ConsumerSecret)))
            return new { error = "ConsumerKey and ConsumerSecret are required for WooCommerce import." };

        return null;
    }

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
            _logger.LogError(ex, "WooCommerce fetch failed for {Url}", fullUri);
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
