using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/woo-lab")]
[Authorize]
public sealed class WooLabController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WooLabController> _logger;

    private static readonly JsonSerializerOptions PrettyJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public WooLabController(IConfiguration configuration, ILogger<WooLabController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("server-health")]
    [AllowAnonymous]
    public IActionResult ServerHealth()
    {
        return Ok(new
        {
            ok = true,
            service = "Kharbarchi.Server",
            time = DateTimeOffset.Now,
            message = "ارتباط پنل مدیریت با سرور داخلی برقرار است."
        });
    }

    [HttpPost("test-woocommerce")]
    public async Task<IActionResult> TestWooCommerce([FromBody] WooLabRequestDto? request, CancellationToken cancellationToken)
    {
        var settings = ResolveSettings(request);
        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            return BadRequest(new { error = "BaseUrl is required. مقدار WooCommerce:BaseUrl را در User Secrets یا فرم تنظیمات وارد کنید." });
        }

        var targetPath = string.IsNullOrWhiteSpace(request?.EndpointPath) ? "wp-json/" : request!.EndpointPath!.TrimStart('/');
        var url = BuildUrl(settings, targetPath, requiresWooKeys: targetPath.Contains("wc/v", StringComparison.OrdinalIgnoreCase));

        using var http = CreateHttpClient(settings);
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyWordPressBasicAuth(message, settings);

        var started = DateTimeOffset.Now;
        using var response = await http.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return StatusCode((int)response.StatusCode, new
        {
            ok = response.IsSuccessStatusCode,
            statusCode = (int)response.StatusCode,
            reason = response.ReasonPhrase,
            elapsedMs = (long)(DateTimeOffset.Now - started).TotalMilliseconds,
            url = MaskSecrets(url),
            message = response.IsSuccessStatusCode ? "اتصال ووکامرس موفق بود." : "اتصال ووکامرس خطا برگشت.",
            body = PrettyJson(body)
        });
    }

    [HttpPost("import-default")]
    public async Task<IActionResult> ImportDefault([FromBody] WooLabRequestDto? request, CancellationToken cancellationToken)
    {
        var settings = ResolveSettings(request);

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            return BadRequest(new { error = "BaseUrl is required. مقدار WooCommerce:BaseUrl را در User Secrets یا فرم تنظیمات وارد کنید." });
        }

        if (string.IsNullOrWhiteSpace(settings.ConsumerKey) || string.IsNullOrWhiteSpace(settings.ConsumerSecret))
        {
            return BadRequest(new
            {
                error = "ConsumerKey and ConsumerSecret are required for WooCommerce import.",
                persian = "برای دریافت محصولات، سفارش‌ها و دسته‌بندی‌های WooCommerce باید Consumer Key و Consumer Secret تنظیم شده باشد. این مقادیر یا از فرم تنظیمات ارسال می‌شوند یا از User Secrets با کلیدهای WooCommerce:ConsumerKey و WooCommerce:ConsumerSecret خوانده می‌شوند."
            });
        }

        await EnsureImportTableAsync(cancellationToken);

        using var http = CreateHttpClient(settings);
        var result = new WooImportResultDto();

        result.Root = await FetchAndStoreAsync(http, settings, "wp-root", "wp-json/", false, cancellationToken);
        result.Categories = await FetchAndStorePagedAsync(http, settings, "category", "wp-json/wc/v3/products/categories", cancellationToken);
        result.Products = await FetchAndStorePagedAsync(http, settings, "product", "wp-json/wc/v3/products", cancellationToken);
        result.Orders = await FetchAndStorePagedAsync(http, settings, "order", "wp-json/wc/v3/orders", cancellationToken);

        return Ok(new
        {
            ok = true,
            message = "دریافت و ذخیره اطلاعات WooCommerce در MySQL انجام شد.",
            result,
            connection = new
            {
                baseUrl = settings.BaseUrl,
                hasConsumerKey = !string.IsNullOrWhiteSpace(settings.ConsumerKey),
                hasConsumerSecret = !string.IsNullOrWhiteSpace(settings.ConsumerSecret),
                hasWordPressBasicAuth = !string.IsNullOrWhiteSpace(settings.WordPressUsername) && !string.IsNullOrWhiteSpace(settings.WordPressPassword)
            }
        });
    }

    [HttpGet("imported-summary")]
    public async Task<IActionResult> ImportedSummary(CancellationToken cancellationToken)
    {
        await EnsureImportTableAsync(cancellationToken);
        var rows = new List<object>();
        await using var connection = new MySqlConnection(GetMySqlConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT SourceType, COUNT(*) AS CountRows, MAX(CreatedAtUtc) AS LastImportedAtUtc
FROM khb_imported_woocommerce_records
GROUP BY SourceType
ORDER BY SourceType;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new
            {
                sourceType = reader.GetString(0),
                countRows = reader.GetInt64(1),
                lastImportedAtUtc = reader.IsDBNull(2) ? null : reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        return Ok(new { ok = true, rows });
    }

    [HttpGet("imported-products")]
    public async Task<IActionResult> ImportedProducts([FromQuery] int take = 25, CancellationToken cancellationToken = default)
    {
        await EnsureImportTableAsync(cancellationToken);
        take = Math.Clamp(take, 1, 100);
        var rows = new List<object>();
        await using var connection = new MySqlConnection(GetMySqlConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT ExternalId, Name, Status, SourceUrl, CreatedAtUtc, RawJson
FROM khb_imported_woocommerce_records
WHERE SourceType = 'product'
ORDER BY Id DESC
LIMIT @take;";
        command.Parameters.AddWithValue("@take", take);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var rawJson = reader.IsDBNull(5) ? "" : reader.GetString(5);
            var product = ExtractProductSummary(rawJson);
            rows.Add(new
            {
                externalId = reader.IsDBNull(0) ? null : reader.GetString(0),
                name = reader.IsDBNull(1) ? product.Name : reader.GetString(1),
                status = reader.IsDBNull(2) ? product.Status : reader.GetString(2),
                sourceUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                createdAtUtc = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss"),
                price = product.Price,
                regularPrice = product.RegularPrice,
                sku = product.Sku,
                stockStatus = product.StockStatus
            });
        }

        return Ok(new { ok = true, rows });
    }

    private async Task<WooFetchSummaryDto> FetchAndStoreAsync(HttpClient http, WooResolvedSettings settings, string sourceType, string endpointPath, bool requiresWooKeys, CancellationToken cancellationToken)
    {
        var url = BuildUrl(settings, endpointPath, requiresWooKeys);
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyWordPressBasicAuth(message, settings);
        using var response = await http.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new WooFetchSummaryDto(sourceType, 0, (int)response.StatusCode, MaskSecrets(url), PrettyJson(body));
        }

        await InsertRawRecordAsync(sourceType, MaskSecrets(url), null, sourceType, response.StatusCode.ToString(), body, cancellationToken);
        return new WooFetchSummaryDto(sourceType, 1, (int)response.StatusCode, MaskSecrets(url), PrettyJson(body));
    }

    private async Task<WooFetchSummaryDto> FetchAndStorePagedAsync(HttpClient http, WooResolvedSettings settings, string sourceType, string endpointPath, CancellationToken cancellationToken)
    {
        var totalStored = 0;
        var statusCode = 200;
        var lastUrl = string.Empty;
        var lastBody = string.Empty;
        var maxPages = settings.MaxPages <= 0 ? 3 : Math.Min(settings.MaxPages, 20);
        var perPage = settings.PerPage <= 0 ? 100 : Math.Clamp(settings.PerPage, 10, 100);

        for (var page = 1; page <= maxPages; page++)
        {
            var path = endpointPath.Contains("?", StringComparison.Ordinal)
                ? $"{endpointPath}&per_page={perPage}&page={page}"
                : $"{endpointPath}?per_page={perPage}&page={page}";

            var url = BuildUrl(settings, path, requiresWooKeys: true);
            lastUrl = MaskSecrets(url);

            using var message = new HttpRequestMessage(HttpMethod.Get, url);
            ApplyWordPressBasicAuth(message, settings);
            using var response = await http.SendAsync(message, cancellationToken);
            statusCode = (int)response.StatusCode;
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            lastBody = PrettyJson(body);

            if (!response.IsSuccessStatusCode)
            {
                break;
            }

            var items = ParseJsonArray(body);
            if (items.Count == 0)
            {
                break;
            }

            foreach (var item in items)
            {
                var id = item["id"]?.ToString();
                var name = item["name"]?.ToString() ?? item["title"]?["rendered"]?.ToString() ?? sourceType;
                var status = item["status"]?.ToString();
                await InsertRawRecordAsync(sourceType, lastUrl, id, name, status, item.ToJsonString(PrettyJsonOptions), cancellationToken);
                totalStored++;
            }

            if (items.Count < perPage)
            {
                break;
            }
        }

        return new WooFetchSummaryDto(sourceType, totalStored, statusCode, lastUrl, lastBody);
    }

    private async Task InsertRawRecordAsync(string sourceType, string sourceUrl, string? externalId, string? name, string? status, string rawJson, CancellationToken cancellationToken)
    {
        await EnsureImportTableAsync(cancellationToken);
        await using var connection = new MySqlConnection(GetMySqlConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        var safeExternalId = string.IsNullOrWhiteSpace(externalId)
    ? $"{sourceType}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Guid.NewGuid():N}"
    : externalId.Trim();
        command.CommandText = @"
INSERT INTO khb_imported_woocommerce_records
(SourceType, SourceUrl, ExternalId, Name, Status, RawJson, ImportedAtUtc, CreatedAtUtc)
VALUES (@SourceType, @SourceUrl, @ExternalId, @Name, @Status, @RawJson, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6));";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("@SourceType", sourceType);
        command.Parameters.AddWithValue("@SourceUrl", string.IsNullOrWhiteSpace(sourceUrl) ? DBNull.Value : sourceUrl);
        command.Parameters.AddWithValue("@ExternalId", string.IsNullOrWhiteSpace(externalId) ? DBNull.Value : externalId);
        command.Parameters.AddWithValue("@Name", string.IsNullOrWhiteSpace(name) ? DBNull.Value : name);
        command.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? DBNull.Value : status);
        command.Parameters.AddWithValue("@RawJson", rawJson ?? "{}");

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task EnsureImportTableAsync(CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(GetMySqlConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS khb_imported_woocommerce_records (
    Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    SourceType VARCHAR(64) NOT NULL,
    SourceUrl TEXT NULL,
    ExternalId VARCHAR(128) NULL,
    Name VARCHAR(512) NULL,
    Status VARCHAR(128) NULL,
    RawJson LONGTEXT NOT NULL,
    CreatedAtUtc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_khb_import_source_type (SourceType),
    INDEX IX_khb_import_external_id (ExternalId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await AddColumnIfMissingAsync(connection, "SourceUrl", "TEXT NULL", cancellationToken);
        await AddColumnIfMissingAsync(connection, "ExternalId", "VARCHAR(128) NULL", cancellationToken);
        await AddColumnIfMissingAsync(connection, "Name", "VARCHAR(512) NULL", cancellationToken);
        await AddColumnIfMissingAsync(connection, "Status", "VARCHAR(128) NULL", cancellationToken);
        await AddColumnIfMissingAsync(connection, "CreatedAtUtc", "DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP", cancellationToken);

        await ModifyColumnAsync(connection, "SourceUrl", "TEXT NULL", cancellationToken);
        await ModifyColumnAsync(connection, "ExternalId", "VARCHAR(191) NULL", cancellationToken);
        await ModifyColumnAsync(connection, "Name", "VARCHAR(512) NULL", cancellationToken);
        await ModifyColumnAsync(connection, "Status", "VARCHAR(128) NULL", cancellationToken);
        await ModifyColumnAsync(connection, "CreatedAtUtc", "DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP", cancellationToken);
    }

    private static async Task ModifyColumnAsync(MySqlConnection connection, string columnName, string definition, CancellationToken cancellationToken)
    {
        await using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE khb_imported_woocommerce_records MODIFY COLUMN `{columnName}` {definition};";
        await alter.ExecuteNonQueryAsync(cancellationToken);
    }
    private static async Task AddColumnIfMissingAsync(MySqlConnection connection, string columnName, string definition, CancellationToken cancellationToken)
    {
        await using var check = connection.CreateCommand();
        check.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'khb_imported_woocommerce_records'
  AND COLUMN_NAME = @ColumnName;";
        check.Parameters.AddWithValue("@ColumnName", columnName);
        var exists = Convert.ToInt32(await check.ExecuteScalarAsync(cancellationToken));
        if (exists > 0)
        {
            return;
        }

        await using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE khb_imported_woocommerce_records ADD COLUMN `{columnName}` {definition};";
        await alter.ExecuteNonQueryAsync(cancellationToken);
    }

    private string GetMySqlConnectionString()
    {
        var value = _configuration.GetConnectionString("MySqlConnection")
                    ?? _configuration.GetConnectionString("DefaultConnection")
                    ?? _configuration["ConnectionStrings:MySqlConnection"];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("MySQL connection string was not found. تنظیم ConnectionStrings:MySqlConnection پیدا نشد.");
        }

        return value;
    }

    private WooResolvedSettings ResolveSettings(WooLabRequestDto? request)
    {
        var timeout = request?.TimeoutSeconds ?? GetInt("WooCommerce:TimeoutSeconds", 60);
        return new WooResolvedSettings
        {
            BaseUrl = FirstNonEmpty(request?.BaseUrl, _configuration["WooCommerce:BaseUrl"]),
            ConsumerKey = FirstNonEmpty(request?.ConsumerKey, _configuration["WooCommerce:ConsumerKey"]),
            ConsumerSecret = FirstNonEmpty(request?.ConsumerSecret, _configuration["WooCommerce:ConsumerSecret"]),
            WordPressUsername = FirstNonEmpty(request?.WordPressUsername, _configuration["WooCommerce:WordPressUsername"], _configuration["WordPress:Username"]),
            WordPressPassword = FirstNonEmpty(request?.WordPressPassword, _configuration["WooCommerce:WordPressPassword"], _configuration["WordPress:Password"]),
            TimeoutSeconds = Math.Clamp(timeout, 5, 300),
            AllowInsecureLocalhostSsl = request?.AllowInsecureLocalhostSsl ?? GetBool("WooCommerce:AllowInsecureLocalhostSsl", true),
            PerPage = request?.PerPage ?? 100,
            MaxPages = request?.MaxPages ?? 3
        };
    }

    private HttpClient CreateHttpClient(WooResolvedSettings settings)
    {
        var handler = new HttpClientHandler();
        if (settings.AllowInsecureLocalhostSsl && IsLocalhost(settings.BaseUrl))
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds) };
    }

    private static void ApplyWordPressBasicAuth(HttpRequestMessage message, WooResolvedSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.WordPressUsername) || string.IsNullOrWhiteSpace(settings.WordPressPassword))
        {
            return;
        }

        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.WordPressUsername}:{settings.WordPressPassword}"));
        message.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
    }

    private static string BuildUrl(WooResolvedSettings settings, string endpointPath, bool requiresWooKeys)
    {
        var baseUrl = (settings.BaseUrl ?? string.Empty).TrimEnd('/');
        var path = endpointPath.TrimStart('/');
        var separator = path.Contains("?", StringComparison.Ordinal) ? "&" : "?";
        var url = $"{baseUrl}/{path}";
        if (requiresWooKeys)
        {
            url += $"{separator}consumer_key={Uri.EscapeDataString(settings.ConsumerKey ?? string.Empty)}&consumer_secret={Uri.EscapeDataString(settings.ConsumerSecret ?? string.Empty)}";
        }

        return url;
    }

    private static bool IsLocalhost(string? baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
               || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
               || uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase);
    }

    private int GetInt(string key, int defaultValue) => int.TryParse(_configuration[key], out var value) ? value : defaultValue;

    private bool GetBool(string key, bool defaultValue) => bool.TryParse(_configuration[key], out var value) ? value : defaultValue;

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }

    private static string PrettyJson(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        try
        {
            var node = JsonNode.Parse(value);
            return node?.ToJsonString(PrettyJsonOptions) ?? value;
        }
        catch
        {
            return value;
        }
    }

    private static string MaskSecrets(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        value = MaskQueryValue(value, "consumer_key");
        value = MaskQueryValue(value, "consumer_secret");
        return value;
    }

    private static string MaskQueryValue(string value, string key)
    {
        var marker = key + "=";
        var index = value.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return value;
        }

        var start = index + marker.Length;
        var end = value.IndexOf('&', start);
        if (end < 0)
        {
            end = value.Length;
        }

        return value[..start] + "***" + value[end..];
    }

    private static List<JsonNode> ParseJsonArray(string body)
    {
        var result = new List<JsonNode>();
        try
        {
            var node = JsonNode.Parse(body);
            if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    if (item is not null)
                    {
                        result.Add(item);
                    }
                }
            }
        }
        catch
        {
            // ignored; caller handles zero items
        }

        return result;
    }

    private static ProductSummary ExtractProductSummary(string rawJson)
    {
        try
        {
            var node = JsonNode.Parse(rawJson);
            return new ProductSummary
            {
                Name = node?["name"]?.ToString(),
                Status = node?["status"]?.ToString(),
                Price = node?["price"]?.ToString(),
                RegularPrice = node?["regular_price"]?.ToString(),
                Sku = node?["sku"]?.ToString(),
                StockStatus = node?["stock_status"]?.ToString()
            };
        }
        catch
        {
            return new ProductSummary();
        }
    }

    private sealed class WooResolvedSettings
    {
        public string? BaseUrl { get; init; }
        public string? ConsumerKey { get; init; }
        public string? ConsumerSecret { get; init; }
        public string? WordPressUsername { get; init; }
        public string? WordPressPassword { get; init; }
        public int TimeoutSeconds { get; init; }
        public bool AllowInsecureLocalhostSsl { get; init; }
        public int PerPage { get; init; }
        public int MaxPages { get; init; }
    }

    private sealed class ProductSummary
    {
        public string? Name { get; init; }
        public string? Status { get; init; }
        public string? Price { get; init; }
        public string? RegularPrice { get; init; }
        public string? Sku { get; init; }
        public string? StockStatus { get; init; }
    }
}

public sealed class WooLabRequestDto
{
    public string? BaseUrl { get; init; }
    public string? ConsumerKey { get; init; }
    public string? ConsumerSecret { get; init; }
    public string? WordPressUsername { get; init; }
    public string? WordPressPassword { get; init; }
    public string? EndpointPath { get; init; }
    public int? TimeoutSeconds { get; init; }
    public bool? AllowInsecureLocalhostSsl { get; init; }
    public int? PerPage { get; init; }
    public int? MaxPages { get; init; }
}

public sealed record WooImportResultDto
{
    public WooFetchSummaryDto? Root { get; set; }
    public WooFetchSummaryDto? Categories { get; set; }
    public WooFetchSummaryDto? Products { get; set; }
    public WooFetchSummaryDto? Orders { get; set; }
}

public sealed record WooFetchSummaryDto(string SourceType, int StoredRows, int StatusCode, string Url, string Body);
