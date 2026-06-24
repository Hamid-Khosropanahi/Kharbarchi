using System.Data.Common;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Kharbarchi.Server.Data;
using Kharbarchi.Server.Models;
using Kharbarchi.Server.Services;
using Kharbarchi.Shared.Contracts.WooCommerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/admin/woocommerce-default-import")]
[Authorize(Roles = "SuperAdmin,Admin,CentralSyncAgent")]
public sealed class WooCommerceDefaultImportController : ControllerBase
{
    private readonly WooCommerceRuntimeSettingsStore _settingsStore;
    private readonly AppDbContext _db;
    private readonly ILogger<WooCommerceDefaultImportController> _logger;

    public WooCommerceDefaultImportController(
        WooCommerceRuntimeSettingsStore settingsStore,
        AppDbContext db,
        ILogger<WooCommerceDefaultImportController> logger)
    {
        _settingsStore = settingsStore;
        _db = db;
        _logger = logger;
    }

    [HttpPost("run")]
    public async Task<ActionResult<WooDefaultImportResultDto>> Run([FromBody] WooDefaultImportRequestDto request, CancellationToken cancellationToken)
    {
        var watch = Stopwatch.StartNew();
        var log = new StringBuilder();
        var result = new WooDefaultImportResultDto();

        try
        {
            var settings = await MergeSettingsAsync(request, cancellationToken);
            if (!IsValidBaseUrl(settings.BaseUrl, out var message))
            {
                return BadRequest(new WooDefaultImportResultDto { Success = false, Message = message });
            }

            request.PerPage = request.PerPage is < 1 or > 100 ? 100 : request.PerPage;
            request.MaxPages = request.MaxPages is < 1 or > 50 ? 10 : request.MaxPages;

            await EnsureImportTableAsync(cancellationToken);

            using var http = CreateHttpClient(settings);
            log.AppendLine($"BaseUrl: {settings.BaseUrl}");
            log.AppendLine($"Table: {result.TableName}");

            // Always save a WordPress API heartbeat row so the database is not empty even before product import.
            var wpRoot = await GetRawAsync(http, settings, "/wp-json/", requiresAuth: false, cancellationToken);
            result.ApiCalls++;
            await SaveRecordAsync("wp-root", "wp-json", "wp-json", "WordPress API Root", wpRoot, cancellationToken);
            log.AppendLine("WordPress API root received and stored.");

            if (request.ImportCategories)
            {
                result.CategoriesImported = await ImportPagedAsync(http, settings, "category", "/wp-json/wc/v3/products/categories", request.PerPage, request.MaxPages, log, cancellationToken);
            }

            if (request.ImportProducts)
            {
                result.ProductsImported = await ImportPagedAsync(http, settings, "product", "/wp-json/wc/v3/products", request.PerPage, request.MaxPages, log, cancellationToken);
            }

            if (request.ImportOrders)
            {
                result.OrdersImported = await ImportPagedAsync(http, settings, "order", "/wp-json/wc/v3/orders?status=any", request.PerPage, request.MaxPages, log, cancellationToken);
            }

            result.ApiCalls += EstimatePagedCalls(result.CategoriesImported, request.PerPage) + EstimatePagedCalls(result.ProductsImported, request.PerPage) + EstimatePagedCalls(result.OrdersImported, request.PerPage);
            watch.Stop();
            result.ElapsedMilliseconds = watch.ElapsedMilliseconds;
            result.Success = true;
            result.Message = "دریافت اطلاعات پایه ووکامرس با موفقیت انجام شد و در MySQL ذخیره شد.";
            result.RawLog = log.ToString();
            return Ok(result);
        }
        catch (Exception ex)
        {
            watch.Stop();
            _logger.LogError(ex, "WooCommerce default import failed.");
            result.Success = false;
            result.ElapsedMilliseconds = watch.ElapsedMilliseconds;
            result.Message = ex.Message;
            result.RawLog = log.AppendLine().AppendLine(ex.ToString()).ToString();
            return StatusCode(500, result);
        }
    }

    private async Task<int> ImportPagedAsync(HttpClient http, WooCommerceRuntimeSettings settings, string sourceType, string endpoint, int perPage, int maxPages, StringBuilder log, CancellationToken cancellationToken)
    {
        var imported = 0;
        for (var page = 1; page <= maxPages; page++)
        {
            var separator = endpoint.Contains('?') ? "&" : "?";
            var relativeUrl = $"{endpoint}{separator}per_page={perPage}&page={page}";
            var raw = await GetRawAsync(http, settings, relativeUrl, requiresAuth: true, cancellationToken);
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                log.AppendLine($"{sourceType}: response was not an array. Endpoint={relativeUrl}");
                await SaveRecordAsync(sourceType + "-raw", relativeUrl, null, relativeUrl, raw, cancellationToken);
                break;
            }

            var count = 0;
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var externalId = TryGetString(item, "id") ?? Guid.NewGuid().ToString("N");
                var slug = TryGetString(item, "slug") ?? TryGetString(item, "number") ?? externalId;
                var title = TryGetString(item, "name") ?? TryGetString(item, "title", "rendered") ?? TryGetString(item, "number") ?? sourceType;
                await SaveRecordAsync(sourceType, externalId, slug, title, item.GetRawText(), cancellationToken);
                imported++;
                count++;
            }

            log.AppendLine($"{sourceType}: page {page}, count {count}");
            if (count < perPage)
            {
                break;
            }
        }

        return imported;
    }

    private async Task<string> GetRawAsync(HttpClient http, WooCommerceRuntimeSettings settings, string relativeUrl, bool requiresAuth, CancellationToken cancellationToken)
    {
        var fullUrl = settings.BaseUrl.TrimEnd('/') + (relativeUrl.StartsWith('/') ? relativeUrl : "/" + relativeUrl);
        using var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        if (requiresAuth)
        {
            ApplyBasicAuth(request, settings);
        }

        using var response = await http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"WooCommerce API failed: {(int)response.StatusCode} {response.ReasonPhrase}\nURL: {fullUrl}\n{body}");
        }

        return body;
    }

    private async Task EnsureImportTableAsync(CancellationToken cancellationToken)
    {
        const string sql = """
CREATE TABLE IF NOT EXISTS `khb_imported_woocommerce_records` (
  `Id` BIGINT NOT NULL AUTO_INCREMENT,
  `SourceType` VARCHAR(64) NOT NULL,
  `ExternalId` VARCHAR(128) NOT NULL,
  `Slug` VARCHAR(255) NULL,
  `Title` VARCHAR(512) NULL,
  `RawJson` LONGTEXT NOT NULL,
  `ImportedAtUtc` DATETIME(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_khb_imported_woocommerce_records_Source_External` (`SourceType`, `ExternalId`)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
""";
        await _db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private async Task SaveRecordAsync(string sourceType, string externalId, string? slug, string? title, string rawJson, CancellationToken cancellationToken)
    {
        var connection = _db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
INSERT INTO `khb_imported_woocommerce_records`
(`SourceType`, `ExternalId`, `Slug`, `Title`, `RawJson`, `ImportedAtUtc`)
VALUES (@sourceType, @externalId, @slug, @title, @rawJson, @importedAtUtc)
ON DUPLICATE KEY UPDATE
`Slug` = VALUES(`Slug`),
`Title` = VALUES(`Title`),
`RawJson` = VALUES(`RawJson`),
`ImportedAtUtc` = VALUES(`ImportedAtUtc`);
""";
        AddParameter(command, "@sourceType", sourceType);
        AddParameter(command, "@externalId", externalId);
        AddParameter(command, "@slug", slug ?? string.Empty);
        AddParameter(command, "@title", title ?? string.Empty);
        AddParameter(command, "@rawJson", rawJson);
        AddParameter(command, "@importedAtUtc", DateTime.UtcNow);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<WooCommerceRuntimeSettings> MergeSettingsAsync(WooDefaultImportRequestDto request, CancellationToken cancellationToken)
    {
        var saved = await _settingsStore.LoadAsync(cancellationToken);
        return new WooCommerceRuntimeSettings
        {
            BaseUrl = string.IsNullOrWhiteSpace(request.BaseUrl) ? saved.BaseUrl : request.BaseUrl.Trim().TrimEnd('/'),
            ConsumerKey = string.IsNullOrWhiteSpace(request.ConsumerKey) ? saved.ConsumerKey : request.ConsumerKey.Trim(),
            ConsumerSecret = string.IsNullOrWhiteSpace(request.ConsumerSecret) ? saved.ConsumerSecret : request.ConsumerSecret.Trim(),
            TimeoutSeconds = request.TimeoutSeconds.GetValueOrDefault(saved.TimeoutSeconds) is var timeout and >= 5 and <= 180 ? timeout : 30,
            AllowInsecureLocalhostSsl = request.AllowInsecureLocalhostSsl.GetValueOrDefault(saved.AllowInsecureLocalhostSsl)
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

    private static bool IsLocalhost(string baseUrl)
    {
        return Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            && (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase));
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

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static string? TryGetString(JsonElement element, params string[] path)
    {
        var current = element;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
            {
                return null;
            }
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Number => current.ToString(),
            _ => current.ToString()
        };
    }

    private static int EstimatePagedCalls(int imported, int perPage) => imported == 0 ? 0 : (int)Math.Ceiling(imported / (double)Math.Max(1, perPage));
}
