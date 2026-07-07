using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Kharbarchi.Server.Infrastructure.Safety;
using Kharbarchi.Server.Options;
using Kharbarchi.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/product-management")]
[Authorize]
public sealed class ProductManagementController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductManagementController> _logger;
    private readonly EnvironmentSafetyGuard _guard;
    private readonly WooCommerceOptions _wooOptions;

    private static readonly JsonSerializerOptions PrettyJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly Regex SafeTableNameRegex = new("^[A-Za-z0-9_]+$", RegexOptions.Compiled);

    public ProductManagementController(
        IConfiguration configuration,
        ILogger<ProductManagementController> logger,
        EnvironmentSafetyGuard guard,
        IOptions<WooCommerceOptions> wooOptions)
    {
        _configuration = configuration;
        _logger = logger;
        _guard = guard;
        _wooOptions = wooOptions.Value;
    }

    [HttpGet("schema/ensure")]
    public async Task<IActionResult> EnsureSchema(CancellationToken cancellationToken)
    {
        await EnsureProductTablesAsync(cancellationToken);
        return Ok(new { ok = true, message = "ساختار جدول‌های مدیریت محصول آماده است." });
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureProductTablesAsync(cancellationToken);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 200);
        var offset = (page - 1) * pageSize;
        var rows = new List<ManagedProductRowDto>();

        using var connection = new MySqlConnection(GetMySqlConnectionString());
        await connection.OpenAsync(cancellationToken);

        var where = new StringBuilder("WHERE 1 = 1 ");
        using var countCommand = connection.CreateCommand();
        using var listCommand = connection.CreateCommand();

        if (!string.IsNullOrWhiteSpace(search))
        {
            where.Append("AND (sp.ProductName LIKE @search OR sp.Sku LIKE @search OR sp.Slug LIKE @search OR sp.PackagingName LIKE @search OR mg.Name LIKE @search) ");
            countCommand.Parameters.AddWithValue("@search", $"%{search.Trim()}%");
            listCommand.Parameters.AddWithValue("@search", $"%{search.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            where.Append("AND sp.Status = @status ");
            countCommand.Parameters.AddWithValue("@status", status.Trim());
            listCommand.Parameters.AddWithValue("@status", status.Trim());
        }

        countCommand.CommandText = $@"
SELECT COUNT(*)
FROM khb_sale_products sp
LEFT JOIN khb_product_main_groups mg ON mg.Id = sp.MainGroupId
{where};";
        var total = Convert.ToInt64(await countCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);

        listCommand.CommandText = $@"
SELECT
    sp.Id,
    sp.WooProductId,
    sp.ProductName,
    sp.Sku,
    sp.Slug,
    sp.Status,
    sp.PackagingName,
    sp.CartonQuantity,
    sp.CashSalePrice,
    sp.InstallmentSalePrice,
    sp.CashPurchasePrice,
    sp.InstallmentPurchasePrice,
    sp.ShortDescription,
    sp.FullDescription,
    sp.ImageUrl,
    sp.CategoriesJson,
    sp.SourceTableName,
    sp.SourceRowHash,
    sp.ImportedAtUtc,
    sp.UpdatedAtUtc,
    mg.Name AS MainGroupName,
    mg.Slug AS MainGroupSlug
FROM khb_sale_products sp
LEFT JOIN khb_product_main_groups mg ON mg.Id = sp.MainGroupId
{where}
ORDER BY sp.UpdatedAtUtc DESC, sp.Id DESC
LIMIT @take OFFSET @skip;";
        listCommand.Parameters.AddWithValue("@take", pageSize);
        listCommand.Parameters.AddWithValue("@skip", offset);

        using var reader = await listCommand.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(ReadProductRow(reader));
        }

        return Ok(new
        {
            ok = true,
            page,
            pageSize,
            total,
            totalPages = (long)Math.Ceiling(total / (double)pageSize),
            rows
        });
    }

    [HttpGet("products/{id:long}")]
    public async Task<IActionResult> GetProduct(long id, CancellationToken cancellationToken)
    {
        await EnsureProductTablesAsync(cancellationToken);
        using var connection = new MySqlConnection(GetMySqlConnectionString());
        await connection.OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    sp.Id,
    sp.WooProductId,
    sp.ExternalId,
    sp.ProductName,
    sp.Sku,
    sp.Slug,
    sp.Status,
    sp.PackagingName,
    sp.CartonQuantity,
    sp.CashSalePrice,
    sp.InstallmentSalePrice,
    sp.CashPurchasePrice,
    sp.InstallmentPurchasePrice,
    sp.ShortDescription,
    sp.FullDescription,
    sp.ImageUrl,
    sp.CategoriesJson,
    sp.SourceTableName,
    sp.SourceRowHash,
    sp.ImportedAtUtc,
    sp.UpdatedAtUtc,
    mg.Name AS MainGroupName,
    mg.Slug AS MainGroupSlug
FROM khb_sale_products sp
LEFT JOIN khb_product_main_groups mg ON mg.Id = sp.MainGroupId
WHERE sp.Id = @id
LIMIT 1;";
        command.Parameters.AddWithValue("@id", id);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return NotFound(new { ok = false, error = "محصول پیدا نشد." });
        }

        return Ok(new { ok = true, product = ReadProductRow(reader) });
    }

    [HttpPut("products/{id:long}")]
    public async Task<IActionResult> UpdateLocalProduct(long id, [FromBody] ManagedProductEditDto request, CancellationToken cancellationToken)
    {
        await EnsureProductTablesAsync(cancellationToken);
        await UpsertLocalProductAsync(id, request, cancellationToken);
        await InsertChangeLogAsync(id, "local-update", "ویرایش مشخصات محصول در پنل مدیریت", JsonSerializer.Serialize(request, PrettyJsonOptions), cancellationToken);
        return Ok(new { ok = true, message = "مشخصات محصول در دیتابیس داخلی ذخیره شد." });
    }

    [HttpPost("import-source-table")]
    public async Task<IActionResult> ImportSourceTable([FromBody] ImportSourceTableRequestDto request, CancellationToken cancellationToken)
    {
        await EnsureProductTablesAsync(cancellationToken);
        var tableName = (request.TableName ?? string.Empty).Trim();
        if (!SafeTableNameRegex.IsMatch(tableName))
        {
            return BadRequest(new { ok = false, error = "نام جدول معتبر نیست. فقط حروف انگلیسی، عدد و _ مجاز است." });
        }

        using var connection = new MySqlConnection(GetMySqlConnectionString());
        await connection.OpenAsync(cancellationToken);
        if (!await TableExistsAsync(connection, tableName, cancellationToken))
        {
            return BadRequest(new { ok = false, error = $"جدول {tableName} در دیتابیس فعلی پیدا نشد." });
        }

        var imported = 0;
        var skipped = 0;
        var maxRows = request.MaxRows <= 0 ? 10000 : Math.Min(request.MaxRows, 50000);
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM `{tableName}` LIMIT @take;";
        command.Parameters.AddWithValue("@take", maxRows);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = ToDictionary(reader);
            var mapped = MapImportedRow(tableName, row);
            if (string.IsNullOrWhiteSpace(mapped.ProductName))
            {
                skipped++;
                continue;
            }

            await reader.DisposeAsync();
            await UpsertMappedImportedProductAsync(connection, mapped, cancellationToken);
            imported++;

            using var nextCommand = connection.CreateCommand();
            nextCommand.CommandText = $"SELECT * FROM `{tableName}` LIMIT @take OFFSET @skip;";
            nextCommand.Parameters.AddWithValue("@take", maxRows - imported - skipped);
            nextCommand.Parameters.AddWithValue("@skip", imported + skipped);
            using var nextReader = await nextCommand.ExecuteReaderAsync(cancellationToken);
            while (await nextReader.ReadAsync(cancellationToken))
            {
                var nextRow = ToDictionary(nextReader);
                var nextMapped = MapImportedRow(tableName, nextRow);
                if (string.IsNullOrWhiteSpace(nextMapped.ProductName))
                {
                    skipped++;
                    continue;
                }
                await UpsertMappedImportedProductAsync(connection, nextMapped, cancellationToken);
                imported++;
            }
            break;
        }

        return Ok(new
        {
            ok = true,
            message = "ورود اطلاعات از جدول منبع انجام شد.",
            tableName,
            imported,
            skipped,
            note = "ستون‌ها با چند نام رایج فارسی/انگلیسی تشخیص داده می‌شوند؛ اگر نام ستون خاص داری، در مرحله بعد mapping اختصاصی اضافه می‌کنیم."
        });
    }

    [HttpPost("woocommerce/import-page")]
    public async Task<IActionResult> ImportWooPage([FromBody] WooPageImportRequestDto request, CancellationToken cancellationToken)
    {
        await EnsureProductTablesAsync(cancellationToken);
        var settings = ResolveWooSettings(request);
        if (string.IsNullOrWhiteSpace(settings.BaseUrl) || string.IsNullOrWhiteSpace(settings.ConsumerKey) || string.IsNullOrWhiteSpace(settings.ConsumerSecret))
        {
            return BadRequest(new { ok = false, error = "BaseUrl, ConsumerKey و ConsumerSecret برای دریافت محصولات لازم است." });
        }

        // Validate WooCommerce target against environment safety policies
        ValidateWooTarget(settings);

        var page = Math.Max(1, request.Page);
        var perPage = Math.Clamp(request.PerPage <= 0 ? 100 : request.PerPage, 10, 100);
        var path = $"wp-json/wc/v3/products?per_page={perPage}&page={page}";
        var url = BuildWooUrl(settings, path, true);
        using var http = CreateHttpClient(settings);
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyWooCommerceBasicAuth(message, settings);
        using var response = await http.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode(
                (int)response.StatusCode,
                new
                {
                    ok = false,
                    url = SanitizeForDisplay(url, settings, 2000),
                    body = SanitizeForDisplay(PrettyJson(body), settings, 20_000)
                });
        }

        var count = 0;
        var changed = 0;
        var node = JsonNode.Parse(body);
        if (node is JsonArray array)
        {
            using var connection = new MySqlConnection(GetMySqlConnectionString());
            await connection.OpenAsync(cancellationToken);
            foreach (var item in array)
            {
                if (item is null)
                {
                    continue;
                }
                var dto = MapWooProduct(item);
                var beforeHash = await GetExistingHashAsync(connection, dto.WooProductId, dto.ExternalId, cancellationToken);
                await UpsertWooProductAsync(connection, dto, item.ToJsonString(PrettyJsonOptions), cancellationToken);
                if (!string.Equals(beforeHash, dto.SourceRowHash, StringComparison.Ordinal))
                {
                    changed++;
                }
                count++;
            }
        }

        return Ok(new
        {
            ok = true,
            page,
            perPage,
            count,
            changed,
            hasMore = count == perPage,
            url = SanitizeForDisplay(url, settings, 2000),
            message = $"صفحه {page} محصولات WooCommerce دریافت شد. تعداد: {count}، تغییرکرده: {changed}"
        });
    }

    [HttpGet("woocommerce/product/{wooProductId:int}")]
    public async Task<IActionResult> GetWooProduct(int wooProductId, CancellationToken cancellationToken)
    {
        var settings = ResolveWooSettings(null);
        if (string.IsNullOrWhiteSpace(settings.BaseUrl) || string.IsNullOrWhiteSpace(settings.ConsumerKey) || string.IsNullOrWhiteSpace(settings.ConsumerSecret))
        {
            return BadRequest(new { ok = false, error = "تنظیمات WooCommerce کامل نیست." });
        }

        ValidateWooTarget(settings);

        var url = BuildWooUrl(settings, $"wp-json/wc/v3/products/{wooProductId}", true);
        using var http = CreateHttpClient(settings);
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyWooCommerceBasicAuth(message, settings);
        using var response = await http.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return StatusCode((int)response.StatusCode, new
        {
            ok = response.IsSuccessStatusCode,
            statusCode = (int)response.StatusCode,
            product = response.IsSuccessStatusCode ? JsonNode.Parse(body) : null,
            raw = SanitizeForDisplay(PrettyJson(body), settings, 20_000)
        });
    }

    [HttpPut("woocommerce/product/{wooProductId:int}")]
    public async Task<IActionResult> UpdateWooProduct(int wooProductId, [FromBody] ManagedProductEditDto request, CancellationToken cancellationToken)
    {
        await EnsureProductTablesAsync(cancellationToken);
        var settings = ResolveWooSettings(null);
        if (string.IsNullOrWhiteSpace(settings.BaseUrl) || string.IsNullOrWhiteSpace(settings.ConsumerKey) || string.IsNullOrWhiteSpace(settings.ConsumerSecret))
        {
            return BadRequest(new { ok = false, error = "تنظیمات WooCommerce کامل نیست." });
        }

        ValidateWooTarget(settings);

        var payload = BuildWooUpdatePayload(request);
        var url = BuildWooUrl(settings, $"wp-json/wc/v3/products/{wooProductId}", true);
        using var http = CreateHttpClient(settings);
        using var message = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(payload.ToJsonString(PrettyJsonOptions), Encoding.UTF8, "application/json")
        };
        ApplyWooCommerceBasicAuth(message, settings);
        using var response = await http.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode(
                (int)response.StatusCode,
                new
                {
                    ok = false,
                    body = SanitizeForDisplay(PrettyJson(body), settings, 20_000)
                });
        }

        var node = JsonNode.Parse(body);
        if (node is not null)
        {
            using var connection = new MySqlConnection(GetMySqlConnectionString());
            await connection.OpenAsync(cancellationToken);
            var mapped = MapWooProduct(node);
            await UpsertWooProductAsync(connection, mapped, node.ToJsonString(PrettyJsonOptions), cancellationToken);
        }

        if (request.Id.HasValue)
        {
            await InsertChangeLogAsync(request.Id.Value, "woo-update", "ارسال تغییرات محصول به WooCommerce", payload.ToJsonString(PrettyJsonOptions), cancellationToken);
        }

        return Ok(new { ok = true, message = "محصول در WooCommerce ذخیره شد.", raw = PrettyJson(body) });
    }

    private async Task EnsureProductTablesAsync(CancellationToken cancellationToken)
    {
        // Runtime schema creation/alteration is disabled. Validate required workflow tables exist.
        try
        {
            await using var connection = new MySqlConnection(GetMySqlConnectionString());
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM khb_product_main_groups LIMIT 1;";
            await command.ExecuteScalarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Do not attempt to create tables at runtime. Instruct operator to apply EF migrations.
            throw new InvalidOperationException("Required product management tables (khb_product_main_groups, khb_sale_products, khb_product_change_log) are missing or have incompatible schema. Apply the reviewed EF Core migration before using Product Management endpoints.", ex);
        }
    }

    private static ManagedProductRowDto ReadProductRow(System.Data.Common.DbDataReader reader)
    {
        return new ManagedProductRowDto
        {
            Id = GetInt64(reader, 0),
            WooProductId = GetNullableInt32(reader, 1),
            ExternalId = GetNullableString(reader, 2),
            ProductName = GetString(reader, 3),
            Sku = GetNullableString(reader, 4),
            Slug = GetNullableString(reader, 5),
            Status = GetNullableString(reader, 6),
            PackagingName = GetNullableString(reader, 7),
            CartonQuantity = GetNullableInt32(reader, 8),
            CashSalePrice = GetNullableDecimal(reader, 9),
            InstallmentSalePrice = GetNullableDecimal(reader, 10),
            CashPurchasePrice = GetNullableDecimal(reader, 11),
            InstallmentPurchasePrice = GetNullableDecimal(reader, 12),
            ShortDescription = GetNullableString(reader, 13),
            FullDescription = GetNullableString(reader, 14),
            ImageUrl = GetNullableString(reader, 15),
            CategoriesJson = GetNullableString(reader, 16),
            SourceTableName = GetNullableString(reader, 17),
            SourceRowHash = GetNullableString(reader, 18),
            ImportedAtUtc = GetNullableDate(reader, 19),
            UpdatedAtUtc = GetNullableDate(reader, 20),
            MainGroupName = GetNullableString(reader, 21),
            MainGroupSlug = GetNullableString(reader, 22)
        };
    }

    private static long GetInt64(System.Data.Common.DbDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? 0 : Convert.ToInt64(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    private static int? GetNullableInt32(System.Data.Common.DbDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    private static decimal? GetNullableDecimal(System.Data.Common.DbDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : Convert.ToDecimal(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    private static string GetString(System.Data.Common.DbDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? string.Empty : Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty;
    private static string? GetNullableString(System.Data.Common.DbDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    private static string? GetNullableDate(System.Data.Common.DbDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : Convert.ToDateTime(reader.GetValue(ordinal), CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    private async Task UpsertLocalProductAsync(long id, ManagedProductEditDto request, CancellationToken cancellationToken)
    {
        using var connection = new MySqlConnection(GetMySqlConnectionString());
        await connection.OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE khb_sale_products
SET ProductName = @ProductName,
    Sku = @Sku,
    Slug = @Slug,
    Status = @Status,
    PackagingName = @PackagingName,
    CartonQuantity = @CartonQuantity,
    CashSalePrice = @CashSalePrice,
    InstallmentSalePrice = @InstallmentSalePrice,
    CashPurchasePrice = @CashPurchasePrice,
    InstallmentPurchasePrice = @InstallmentPurchasePrice,
    ShortDescription = @ShortDescription,
    FullDescription = @FullDescription,
    ImageUrl = @ImageUrl,
    CategoriesJson = @CategoriesJson,
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE Id = @Id;";
        AddProductParameters(command, request);
        command.Parameters.AddWithValue("@Id", id);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddProductParameters(MySqlCommand command, ManagedProductEditDto request, bool clear = true)
    {
        if (clear)
        {
            command.Parameters.Clear();
        }
        command.Parameters.AddWithValue("@ProductName", DbValue(request.ProductName));
        command.Parameters.AddWithValue("@Sku", DbValue(request.Sku));
        command.Parameters.AddWithValue("@Slug", DbValue(request.Slug));
        command.Parameters.AddWithValue("@Status", DbValue(request.Status));
        command.Parameters.AddWithValue("@PackagingName", DbValue(request.PackagingName));
        command.Parameters.AddWithValue("@CartonQuantity", request.CartonQuantity.HasValue ? request.CartonQuantity.Value : DBNull.Value);
        command.Parameters.AddWithValue("@CashSalePrice", request.CashSalePrice.HasValue ? request.CashSalePrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("@InstallmentSalePrice", request.InstallmentSalePrice.HasValue ? request.InstallmentSalePrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("@CashPurchasePrice", request.CashPurchasePrice.HasValue ? request.CashPurchasePrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("@InstallmentPurchasePrice", request.InstallmentPurchasePrice.HasValue ? request.InstallmentPurchasePrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("@ShortDescription", DbValue(request.ShortDescription));
        command.Parameters.AddWithValue("@FullDescription", DbValue(request.FullDescription));
        command.Parameters.AddWithValue("@ImageUrl", DbValue(request.ImageUrl));
        command.Parameters.AddWithValue("@CategoriesJson", DbValue(request.CategoriesJson));
    }

    private async Task<bool> TableExistsAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @table;";
        command.Parameters.AddWithValue("@table", tableName);
        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        return count > 0;
    }

    private static Dictionary<string, string?> ToDictionary(System.Data.Common.DbDataReader reader)
    {
        var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < reader.FieldCount; i++)
        {
            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture);
        }
        return row;
    }

    private static MappedImportedProduct MapImportedRow(string tableName, IReadOnlyDictionary<string, string?> row)
    {
        var id = First(row, "Id", "ID", "ProductId", "WooProductId", "Code", "کد", "شناسه") ?? Guid.NewGuid().ToString("N");
        var family = First(row, "MainProduct", "Family", "Commodity", "ProductGroup", "Category", "دسته", "گروه", "محصول اصلی")
            ?? First(row, "ProductName", "Name", "نام کالا", "نام محصول")
            ?? "عمومی";
        var name = First(row, "ProductName", "Name", "Title", "PersianName", "FaName", "نام کالا", "نام محصول", "عنوان");
        var english = First(row, "EnglishName", "EnName", "LatinName", "نام انگلیسی");
        var packaging = First(row, "Packaging", "Package", "PackageName", "بسته بندی", "نوع بسته بندی")
            ?? First(row, "Weight", "وزن");
        return new MappedImportedProduct
        {
            SourceTableName = tableName,
            ExternalId = id,
            MainGroupName = family,
            MainGroupSlug = Slugify(First(row, "GroupSlug", "CategorySlug", "En_Taxonomic", "Taxonomic") ?? family),
            ProductName = name ?? family,
            EnglishName = english,
            Sku = First(row, "Sku", "SKU", "کد کالا"),
            Slug = Slugify(First(row, "Slug", "EnglishSlug") ?? english ?? name ?? family),
            Status = First(row, "Status", "وضعیت") ?? "draft",
            PackagingName = packaging,
            CartonQuantity = ToInt(First(row, "CartonQuantity", "QuantityInBox", "PacksPerCarton", "تعداد کارتن", "تعداد در کارتن")),
            CashSalePrice = ToDecimal(First(row, "CashSalePrice", "SaleCashPrice", "RegularPrice", "قیمت فروش نقد")),
            InstallmentSalePrice = ToDecimal(First(row, "InstallmentSalePrice", "SaleInstallmentPrice", "قیمت فروش شرایطی")),
            CashPurchasePrice = ToDecimal(First(row, "CashPurchasePrice", "PurchaseCashPrice", "قیمت خرید نقد")),
            InstallmentPurchasePrice = ToDecimal(First(row, "InstallmentPurchasePrice", "PurchaseInstallmentPrice", "قیمت خرید شرایطی")),
            ShortDescription = First(row, "ShortDescription", "ShortDesc", "توضیحات کوتاه"),
            FullDescription = First(row, "Description", "FullDescription", "توضیحات", "توضیحات کلی"),
            ImageUrl = First(row, "ImageUrl", "Image", "تصویر"),
            CategoriesJson = First(row, "CategoriesJson", "Categories", "دسته بندی"),
            SourceRowHash = Hash(JsonSerializer.Serialize(row, PrettyJsonOptions))
        };
    }

    private async Task UpsertMappedImportedProductAsync(MySqlConnection connection, MappedImportedProduct product, CancellationToken cancellationToken)
    {
        var groupId = await UpsertMainGroupAsync(connection, product.MainGroupName, product.EnglishName, product.MainGroupSlug, product.SourceTableName, cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO khb_sale_products
(MainGroupId, SourceTableName, ExternalId, SourceRowHash, ProductName, Sku, Slug, Status, PackagingName, CartonQuantity,
 CashSalePrice, InstallmentSalePrice, CashPurchasePrice, InstallmentPurchasePrice, ShortDescription, FullDescription, ImageUrl, CategoriesJson, ImportedAtUtc, UpdatedAtUtc)
VALUES
(@MainGroupId, @SourceTableName, @ExternalId, @SourceRowHash, @ProductName, @Sku, @Slug, @Status, @PackagingName, @CartonQuantity,
 @CashSalePrice, @InstallmentSalePrice, @CashPurchasePrice, @InstallmentPurchasePrice, @ShortDescription, @FullDescription, @ImageUrl, @CategoriesJson, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE
    MainGroupId = VALUES(MainGroupId),
    SourceRowHash = VALUES(SourceRowHash),
    ProductName = VALUES(ProductName),
    Sku = VALUES(Sku),
    Slug = VALUES(Slug),
    Status = VALUES(Status),
    PackagingName = VALUES(PackagingName),
    CartonQuantity = VALUES(CartonQuantity),
    CashSalePrice = VALUES(CashSalePrice),
    InstallmentSalePrice = VALUES(InstallmentSalePrice),
    CashPurchasePrice = VALUES(CashPurchasePrice),
    InstallmentPurchasePrice = VALUES(InstallmentPurchasePrice),
    ShortDescription = VALUES(ShortDescription),
    FullDescription = VALUES(FullDescription),
    ImageUrl = VALUES(ImageUrl),
    CategoriesJson = VALUES(CategoriesJson),
    UpdatedAtUtc = UTC_TIMESTAMP(6);";
        command.Parameters.AddWithValue("@MainGroupId", groupId);
        command.Parameters.AddWithValue("@SourceTableName", product.SourceTableName);
        command.Parameters.AddWithValue("@ExternalId", product.ExternalId);
        command.Parameters.AddWithValue("@SourceRowHash", product.SourceRowHash);
        command.Parameters.AddWithValue("@ProductName", product.ProductName);
        command.Parameters.AddWithValue("@Sku", DbValue(product.Sku));
        command.Parameters.AddWithValue("@Slug", DbValue(product.Slug));
        command.Parameters.AddWithValue("@Status", DbValue(product.Status));
        command.Parameters.AddWithValue("@PackagingName", DbValue(product.PackagingName));
        command.Parameters.AddWithValue("@CartonQuantity", product.CartonQuantity.HasValue ? product.CartonQuantity.Value : DBNull.Value);
        command.Parameters.AddWithValue("@CashSalePrice", product.CashSalePrice.HasValue ? product.CashSalePrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("@InstallmentSalePrice", product.InstallmentSalePrice.HasValue ? product.InstallmentSalePrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("@CashPurchasePrice", product.CashPurchasePrice.HasValue ? product.CashPurchasePrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("@InstallmentPurchasePrice", product.InstallmentPurchasePrice.HasValue ? product.InstallmentPurchasePrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("@ShortDescription", DbValue(product.ShortDescription));
        command.Parameters.AddWithValue("@FullDescription", DbValue(product.FullDescription));
        command.Parameters.AddWithValue("@ImageUrl", DbValue(product.ImageUrl));
        command.Parameters.AddWithValue("@CategoriesJson", DbValue(product.CategoriesJson));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<long> UpsertMainGroupAsync(MySqlConnection connection, string name, string? englishName, string? slug, string? sourceTable, CancellationToken cancellationToken)
    {
        var sourceKey = Slugify(slug ?? englishName ?? name);
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
INSERT INTO khb_product_main_groups (SourceTableName, SourceKey, Name, EnglishName, Slug, UpdatedAtUtc)
VALUES (@SourceTableName, @SourceKey, @Name, @EnglishName, @Slug, UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE
    Name = VALUES(Name),
    EnglishName = VALUES(EnglishName),
    Slug = VALUES(Slug),
    UpdatedAtUtc = UTC_TIMESTAMP(6);";
            command.Parameters.AddWithValue("@SourceTableName", DbValue(sourceTable));
            command.Parameters.AddWithValue("@SourceKey", sourceKey);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@EnglishName", DbValue(englishName));
            command.Parameters.AddWithValue("@Slug", DbValue(slug ?? sourceKey));
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT Id FROM khb_product_main_groups WHERE SourceKey = @SourceKey LIMIT 1;";
            command.Parameters.AddWithValue("@SourceKey", sourceKey);
            return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }
    }

    private static ManagedProductEditDto MapWooProduct(JsonNode product)
    {
        var categories = product["categories"]?.ToJsonString(PrettyJsonOptions);
        var images = product["images"] as JsonArray;
        var imageUrl = images is { Count: > 0 } ? images[0]?["src"]?.GetValue<string>() : null;
        var meta = product["meta_data"] as JsonArray;
        string? MetaValue(string key)
        {
            if (meta is null) return null;
            foreach (var item in meta)
            {
                if (string.Equals(item?["key"]?.GetValue<string>(), key, StringComparison.OrdinalIgnoreCase))
                {
                    return item?["value"]?.ToString();
                }
            }
            return null;
        }

        var dto = new ManagedProductEditDto
        {
            WooProductId = product["id"]?.GetValue<int>(),
            ExternalId = product["id"]?.ToString(),
            ProductName = product["name"]?.GetValue<string>() ?? string.Empty,
            Sku = product["sku"]?.GetValue<string>(),
            Slug = product["slug"]?.GetValue<string>(),
            Status = product["status"]?.GetValue<string>(),
            CashSalePrice = ToDecimal(product["regular_price"]?.GetValue<string>() ?? product["price"]?.GetValue<string>()),
            InstallmentSalePrice = ToDecimal(MetaValue("_khb_installment_sale_price")),
            CashPurchasePrice = ToDecimal(MetaValue("_khb_cash_purchase_price")),
            InstallmentPurchasePrice = ToDecimal(MetaValue("_khb_installment_purchase_price")),
            CartonQuantity = ToInt(MetaValue("_khb_carton_quantity")),
            PackagingName = MetaValue("_khb_packaging_name"),
            ShortDescription = product["short_description"]?.GetValue<string>(),
            FullDescription = product["description"]?.GetValue<string>(),
            ImageUrl = imageUrl,
            CategoriesJson = categories
        };
        dto.SourceRowHash = Hash(product.ToJsonString(PrettyJsonOptions));
        return dto;
    }

    private async Task<string?> GetExistingHashAsync(MySqlConnection connection, int? wooProductId, string? externalId, CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT SourceRowHash FROM khb_sale_products
WHERE (WooProductId = @WooProductId AND @WooProductId IS NOT NULL) OR (ExternalId = @ExternalId AND @ExternalId IS NOT NULL)
LIMIT 1;";
        command.Parameters.AddWithValue("@WooProductId", wooProductId.HasValue ? wooProductId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@ExternalId", DbValue(externalId));
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : Convert.ToString(result, CultureInfo.InvariantCulture);
    }

    private async Task UpsertWooProductAsync(MySqlConnection connection, ManagedProductEditDto product, string rawJson, CancellationToken cancellationToken)
    {
        var mainGroupName = ExtractMainGroupName(product.CategoriesJson) ?? "WooCommerce";
        var groupId = await UpsertMainGroupAsync(connection, mainGroupName, null, Slugify(mainGroupName), "woocommerce", cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO khb_sale_products
(WooProductId, ExternalId, MainGroupId, SourceTableName, SourceRowHash, ProductName, Sku, Slug, Status, PackagingName, CartonQuantity,
 CashSalePrice, InstallmentSalePrice, CashPurchasePrice, InstallmentPurchasePrice, ShortDescription, FullDescription, ImageUrl, CategoriesJson, WooRawJson, ImportedAtUtc, UpdatedAtUtc)
VALUES
(@WooProductId, @ExternalId, @MainGroupId, 'woocommerce', @SourceRowHash, @ProductName, @Sku, @Slug, @Status, @PackagingName, @CartonQuantity,
 @CashSalePrice, @InstallmentSalePrice, @CashPurchasePrice, @InstallmentPurchasePrice, @ShortDescription, @FullDescription, @ImageUrl, @CategoriesJson, @WooRawJson, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE
    MainGroupId = VALUES(MainGroupId),
    SourceRowHash = VALUES(SourceRowHash),
    ProductName = VALUES(ProductName),
    Sku = VALUES(Sku),
    Slug = VALUES(Slug),
    Status = VALUES(Status),
    PackagingName = VALUES(PackagingName),
    CartonQuantity = VALUES(CartonQuantity),
    CashSalePrice = VALUES(CashSalePrice),
    InstallmentSalePrice = VALUES(InstallmentSalePrice),
    CashPurchasePrice = VALUES(CashPurchasePrice),
    InstallmentPurchasePrice = VALUES(InstallmentPurchasePrice),
    ShortDescription = VALUES(ShortDescription),
    FullDescription = VALUES(FullDescription),
    ImageUrl = VALUES(ImageUrl),
    CategoriesJson = VALUES(CategoriesJson),
    WooRawJson = VALUES(WooRawJson),
    ImportedAtUtc = UTC_TIMESTAMP(6),
    UpdatedAtUtc = UTC_TIMESTAMP(6);";
        command.Parameters.AddWithValue("@WooProductId", product.WooProductId.HasValue ? product.WooProductId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@ExternalId", DbValue(product.ExternalId ?? product.WooProductId?.ToString(CultureInfo.InvariantCulture)));
        command.Parameters.AddWithValue("@MainGroupId", groupId);
        command.Parameters.AddWithValue("@SourceRowHash", DbValue(product.SourceRowHash));
        AddProductParameters(command, product, clear: false);
        command.Parameters.AddWithValue("@WooRawJson", rawJson);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static JsonObject BuildWooUpdatePayload(ManagedProductEditDto request)
    {
        var payload = new JsonObject();
        if (!string.IsNullOrWhiteSpace(request.ProductName)) payload["name"] = request.ProductName;
        if (!string.IsNullOrWhiteSpace(request.Slug)) payload["slug"] = request.Slug;
        if (!string.IsNullOrWhiteSpace(request.Status)) payload["status"] = request.Status;
        if (!string.IsNullOrWhiteSpace(request.ShortDescription)) payload["short_description"] = request.ShortDescription;
        if (!string.IsNullOrWhiteSpace(request.FullDescription)) payload["description"] = request.FullDescription;
        if (!string.IsNullOrWhiteSpace(request.Sku)) payload["sku"] = request.Sku;
        if (request.CashSalePrice.HasValue) payload["regular_price"] = request.CashSalePrice.Value.ToString("0.##", CultureInfo.InvariantCulture);

        var meta = new JsonArray
        {
            new JsonObject { ["key"] = "_khb_installment_sale_price", ["value"] = request.InstallmentSalePrice?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty },
            new JsonObject { ["key"] = "_khb_cash_purchase_price", ["value"] = request.CashPurchasePrice?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty },
            new JsonObject { ["key"] = "_khb_installment_purchase_price", ["value"] = request.InstallmentPurchasePrice?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty },
            new JsonObject { ["key"] = "_khb_carton_quantity", ["value"] = request.CartonQuantity?.ToString(CultureInfo.InvariantCulture) ?? string.Empty },
            new JsonObject { ["key"] = "_khb_packaging_name", ["value"] = request.PackagingName ?? string.Empty }
        };
        payload["meta_data"] = meta;

        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            payload["images"] = new JsonArray { new JsonObject { ["src"] = request.ImageUrl } };
        }

        return payload;
    }

    private async Task InsertChangeLogAsync(long productId, string changeType, string summary, string payload, CancellationToken cancellationToken)
    {
        using var connection = new MySqlConnection(GetMySqlConnectionString());
        await connection.OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO khb_product_change_log (ProductId, ChangeType, Summary, Payload, CreatedAtUtc)
VALUES (@ProductId, @ChangeType, @Summary, @Payload, UTC_TIMESTAMP(6));";
        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue("@ChangeType", changeType);
        command.Parameters.AddWithValue("@Summary", summary);
        command.Parameters.AddWithValue("@Payload", payload);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private WooSettings ResolveWooSettings(WooPageImportRequestDto? request)
    {
        return new WooSettings
        {
            BaseUrl = FirstNonEmpty(request?.BaseUrl, _configuration["WooCommerce:BaseUrl"]),
            ConsumerKey = FirstNonEmpty(request?.ConsumerKey, _configuration["WooCommerce:ConsumerKey"]),
            ConsumerSecret = FirstNonEmpty(request?.ConsumerSecret, _configuration["WooCommerce:ConsumerSecret"]),
            WordPressUsername = FirstNonEmpty(request?.WordPressUsername, _configuration["WooCommerce:WordPressUsername"]),
            WordPressPassword = FirstNonEmpty(request?.WordPressPassword, _configuration["WooCommerce:WordPressPassword"]),
            TimeoutSeconds = request?.TimeoutSeconds > 0 ? request.TimeoutSeconds : GetIntConfig("WooCommerce:TimeoutSeconds", 600),
            AllowInsecureLocalhostSsl = request?.AllowInsecureLocalhostSsl ?? GetBoolConfig("WooCommerce:AllowInsecureLocalhostSsl", true)
        };
    }

    private HttpClient CreateHttpClient(WooSettings settings)
    {
        var handler = new HttpClientHandler();
        if (settings.AllowInsecureLocalhostSsl && IsLocalhost(settings.BaseUrl))
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }
        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds, 30, 900))
        };
    }

    private void ValidateWooTarget(WooSettings settings)
    {
        _guard.ValidateWooProfile(
            settings.BaseUrl ?? string.Empty,
            !settings.AllowInsecureLocalhostSsl,
            _wooOptions.EnvironmentType,
            expectedProductionBaseUrl: _wooOptions.BaseUrl);
    }

    private static bool IsLocalhost(string? baseUrl) =>
        Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
        && (uri.IsLoopback || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase));

    private static void ApplyWooCommerceBasicAuth(HttpRequestMessage message, WooSettings settings)
    {
        WooCommerceRequestSecurity.ApplyBasicAuthentication(
            message,
            settings.ConsumerKey,
            settings.ConsumerSecret);
    }

    private static string BuildWooUrl(WooSettings settings, string endpointPath, bool requiresWooKeys)
    {
        WooCommerceRequestSecurity.RejectCredentialQueryParameters(settings.BaseUrl);
        WooCommerceRequestSecurity.RejectCredentialQueryParameters(endpointPath);
        var baseUrl = (settings.BaseUrl ?? string.Empty).TrimEnd('/');
        var path = endpointPath.TrimStart('/');
        var url = $"{baseUrl}/{path}";
        // Do not append consumer_key/consumer_secret to the URL. Send consumer credentials with the Basic Authorization header.
        return url;
    }

    private static string SanitizeForDisplay(
        string? value,
        WooSettings settings,
        int maxLength) =>
        WooCommerceRequestSecurity.Sanitize(
            value,
            settings.ConsumerKey,
            settings.ConsumerSecret,
            maxLength);

    private static string MaskSecrets(string url)
    {
        return Regex.Replace(url, "(consumer_key|consumer_secret)=([^&]+)", "$1=***", RegexOptions.IgnoreCase);
    }

    private string GetMySqlConnectionString()
    {
        var connectionString = _configuration.GetConnectionString("MySqlConnection") ?? _configuration["ConnectionStrings:MySqlConnection"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:MySqlConnection is not configured.");
        }
        return connectionString.Replace("SslMode=None", "SslMode=Disabled", StringComparison.OrdinalIgnoreCase);
    }

    private static string PrettyJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        try
        {
            var node = JsonNode.Parse(text);
            return node?.ToJsonString(PrettyJsonOptions) ?? text;
        }
        catch
        {
            return text;
        }
    }

    private static string? First(IReadOnlyDictionary<string, string?> row, params string[] names)
    {
        foreach (var name in names)
        {
            if (row.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)) return value.Trim();
        }
        return null;
    }

    private static string? FirstNonEmpty(params string?[] values) => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();
    private int GetIntConfig(string key, int fallback) => int.TryParse(_configuration[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;
    private bool GetBoolConfig(string key, bool fallback) => bool.TryParse(_configuration[key], out var value) ? value : fallback;

    private static int? ToInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Replace(",", string.Empty, StringComparison.Ordinal).Trim();
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }

    private static decimal? ToDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Replace(",", string.Empty, StringComparison.Ordinal).Replace("٬", string.Empty, StringComparison.Ordinal).Trim();
        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }

    private static string Slugify(string? value)
    {
        var text = string.IsNullOrWhiteSpace(value) ? "product" : value.Trim().ToLowerInvariant();
        text = Regex.Replace(text, "[^a-z0-9\u0600-\u06FF]+", "-");
        return text.Trim('-');
    }

    private static string Hash(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }

    private static object DbValue(string? value) => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();

    private static string? ExtractMainGroupName(string? categoriesJson)
    {
        if (string.IsNullOrWhiteSpace(categoriesJson)) return null;
        try
        {
            var node = JsonNode.Parse(categoriesJson) as JsonArray;
            return node?.FirstOrDefault()?["name"]?.GetValue<string>();
        }
        catch
        {
            return null;
        }
    }

    private sealed class WooSettings
    {
        public string? BaseUrl { get; set; }
        public string? ConsumerKey { get; set; }
        public string? ConsumerSecret { get; set; }
        public string? WordPressUsername { get; set; }
        public string? WordPressPassword { get; set; }
        public int TimeoutSeconds { get; set; } = 600;
        public bool AllowInsecureLocalhostSsl { get; set; } = true;
    }

    private sealed class MappedImportedProduct : ManagedProductEditDto
    {
        public string SourceTableName { get; set; } = string.Empty;
        public string MainGroupName { get; set; } = string.Empty;
        public string? MainGroupSlug { get; set; }
        public string? EnglishName { get; set; }
    }
}

public sealed class ImportSourceTableRequestDto
{
    public string? TableName { get; set; }
    public int MaxRows { get; set; } = 10000;
}

public class WooPageImportRequestDto
{
    public string? BaseUrl { get; set; }
    public string? ConsumerKey { get; set; }
    public string? ConsumerSecret { get; set; }
    public string? WordPressUsername { get; set; }
    public string? WordPressPassword { get; set; }
    public int TimeoutSeconds { get; set; } = 600;
    public bool AllowInsecureLocalhostSsl { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 100;
}

public sealed class ManagedProductRowDto : ManagedProductEditDto
{
    public string? MainGroupName { get; set; }
    public string? MainGroupSlug { get; set; }
    public string? SourceTableName { get; set; }
    public string? ImportedAtUtc { get; set; }
    public string? UpdatedAtUtc { get; set; }
}

public class ManagedProductEditDto
{
    public long? Id { get; set; }
    public int? WooProductId { get; set; }
    public string? ExternalId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Slug { get; set; }
    public string? Status { get; set; }
    public string? PackagingName { get; set; }
    public int? CartonQuantity { get; set; }
    public decimal? CashSalePrice { get; set; }
    public decimal? InstallmentSalePrice { get; set; }
    public decimal? CashPurchasePrice { get; set; }
    public decimal? InstallmentPurchasePrice { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? ImageUrl { get; set; }
    public string? CategoriesJson { get; set; }
    public string? SourceRowHash { get; set; }
}
