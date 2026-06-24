using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/product-csv")]
public sealed class ProductCsvImportController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductCsvImportController> _logger;

    private const string DefaultTableName = "All_Product_With_Process";

    public ProductCsvImportController(IConfiguration configuration, ILogger<ProductCsvImportController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("import-all-product-with-process")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> ImportAllProductWithProcess(
        IFormFile file,
        [FromQuery] string? tableName,
        [FromQuery] bool truncate = false,
        CancellationToken cancellationToken = default)
    {
        var safeTableName = NormalizeTableName(tableName);

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "CSV file is required." });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only CSV files are supported. Save Excel as CSV UTF-8." });
        }

        var csvText = await ReadCsvAsUtf8Async(file, cancellationToken);
        if (csvText.Contains('\uFFFD'))
        {
            return BadRequest(new
            {
                error = "CSV encoding is not valid UTF-8. In Excel use: Save As > CSV UTF-8 (Comma delimited)."
            });
        }

        var parsed = CsvParser.Parse(csvText);
        if (parsed.Headers.Count == 0)
        {
            return BadRequest(new { error = "CSV header row was not found." });
        }

        if (parsed.Rows.Count == 0)
        {
            return BadRequest(new { error = "CSV has headers but no data rows." });
        }

        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        await EnsureAllProductTableAsync(connection, safeTableName, cancellationToken);
        await EnsureProductManagementTablesAsync(connection, cancellationToken);

        if (truncate)
        {
            await ExecuteAsync(connection, $"TRUNCATE TABLE `{safeTableName}`;", cancellationToken);
        }

        var batchId = Guid.NewGuid().ToString("N");
        var inserted = 0;
        var updated = 0;
        var skipped = 0;
        var warnings = new List<string>();

        for (var i = 0; i < parsed.Rows.Count; i++)
        {
            var row = parsed.Rows[i];
            var rowDictionary = CsvParser.ToDictionary(parsed.Headers, row);
            var mapped = ProductRowMapper.Map(rowDictionary);

            if (string.IsNullOrWhiteSpace(mapped.ProductName))
            {
                skipped++;
                warnings.Add($"Row {i + 2}: product name is empty.");
                continue;
            }

            var rawJson = JsonSerializer.Serialize(rowDictionary, JsonOptions);
            var rowHash = ComputeSha256(rawJson);

            await using var command = connection.CreateCommand();
            command.CommandText = $@"
INSERT INTO `{safeTableName}`
(
    ImportBatchId,
    SourceRowNumber,
    SourceRowHash,
    RawJson,
    MainProductName,
    MainProductSlug,
    GroupName,
    CategoryName,
    CategorySlug,
    ProductName,
    ProductEnglishName,
    ProductSlug,
    SKU,
    BrandName,
    PackageName,
    UnitWeight,
    PacksPerCarton,
    CartonQuantity,
    SalePriceCash,
    SalePriceInstallment,
    PurchasePriceCash,
    PurchasePriceInstallment,
    ShortDescription,
    FullDescription,
    ImageUrl,
    GalleryJson,
    Status,
    WooProductId,
    HaveOtherPackage,
    PackageOne,
    CreatedAtUtc,
    UpdatedAtUtc
)
VALUES
(
    @ImportBatchId,
    @SourceRowNumber,
    @SourceRowHash,
    @RawJson,
    @MainProductName,
    @MainProductSlug,
    @GroupName,
    @CategoryName,
    @CategorySlug,
    @ProductName,
    @ProductEnglishName,
    @ProductSlug,
    @SKU,
    @BrandName,
    @PackageName,
    @UnitWeight,
    @PacksPerCarton,
    @CartonQuantity,
    @SalePriceCash,
    @SalePriceInstallment,
    @PurchasePriceCash,
    @PurchasePriceInstallment,
    @ShortDescription,
    @FullDescription,
    @ImageUrl,
    @GalleryJson,
    @Status,
    @WooProductId,
    @HaveOtherPackage,
    @PackageOne,
    UTC_TIMESTAMP(6),
    UTC_TIMESTAMP(6)
)
ON DUPLICATE KEY UPDATE
    ImportBatchId = VALUES(ImportBatchId),
    SourceRowNumber = VALUES(SourceRowNumber),
    RawJson = VALUES(RawJson),
    MainProductName = VALUES(MainProductName),
    MainProductSlug = VALUES(MainProductSlug),
    GroupName = VALUES(GroupName),
    CategoryName = VALUES(CategoryName),
    CategorySlug = VALUES(CategorySlug),
    ProductName = VALUES(ProductName),
    ProductEnglishName = VALUES(ProductEnglishName),
    ProductSlug = VALUES(ProductSlug),
    SKU = VALUES(SKU),
    BrandName = VALUES(BrandName),
    PackageName = VALUES(PackageName),
    UnitWeight = VALUES(UnitWeight),
    PacksPerCarton = VALUES(PacksPerCarton),
    CartonQuantity = VALUES(CartonQuantity),
    SalePriceCash = VALUES(SalePriceCash),
    SalePriceInstallment = VALUES(SalePriceInstallment),
    PurchasePriceCash = VALUES(PurchasePriceCash),
    PurchasePriceInstallment = VALUES(PurchasePriceInstallment),
    ShortDescription = VALUES(ShortDescription),
    FullDescription = VALUES(FullDescription),
    ImageUrl = VALUES(ImageUrl),
    GalleryJson = VALUES(GalleryJson),
    Status = VALUES(Status),
    WooProductId = VALUES(WooProductId),
    HaveOtherPackage = VALUES(HaveOtherPackage),
    PackageOne = VALUES(PackageOne),
    UpdatedAtUtc = UTC_TIMESTAMP(6);";

            Add(command, "@ImportBatchId", batchId);
            Add(command, "@SourceRowNumber", i + 2);
            Add(command, "@SourceRowHash", rowHash);
            Add(command, "@RawJson", rawJson);
            Add(command, "@MainProductName", mapped.MainProductName);
            Add(command, "@MainProductSlug", mapped.MainProductSlug);
            Add(command, "@GroupName", mapped.GroupName);
            Add(command, "@CategoryName", mapped.CategoryName);
            Add(command, "@CategorySlug", mapped.CategorySlug);
            Add(command, "@ProductName", mapped.ProductName);
            Add(command, "@ProductEnglishName", mapped.ProductEnglishName);
            Add(command, "@ProductSlug", mapped.ProductSlug);
            Add(command, "@SKU", mapped.Sku);
            Add(command, "@BrandName", mapped.BrandName);
            Add(command, "@PackageName", mapped.PackageName);
            Add(command, "@UnitWeight", mapped.UnitWeight);
            Add(command, "@PacksPerCarton", mapped.PacksPerCarton);
            Add(command, "@CartonQuantity", mapped.CartonQuantity);
            Add(command, "@SalePriceCash", mapped.SalePriceCash);
            Add(command, "@SalePriceInstallment", mapped.SalePriceInstallment);
            Add(command, "@PurchasePriceCash", mapped.PurchasePriceCash);
            Add(command, "@PurchasePriceInstallment", mapped.PurchasePriceInstallment);
            Add(command, "@ShortDescription", mapped.ShortDescription);
            Add(command, "@FullDescription", mapped.FullDescription);
            Add(command, "@ImageUrl", mapped.ImageUrl);
            Add(command, "@GalleryJson", mapped.GalleryJson);
            Add(command, "@Status", mapped.Status);
            Add(command, "@WooProductId", mapped.WooProductId);
            Add(command, "@HaveOtherPackage", mapped.HaveOtherPackage);
            Add(command, "@PackageOne", mapped.PackageOne);

            var affected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (affected == 1)
            {
                inserted++;
            }
            else
            {
                updated++;
            }
        }

        return Ok(new
        {
            tableName = safeTableName,
            batchId,
            file = file.FileName,
            headers = parsed.Headers.Count,
            rows = parsed.Rows.Count,
            inserted,
            updated,
            skipped,
            warnings = warnings.Take(20).ToArray()
        });
    }

    [HttpPost("process-all-product-with-process")]
    public async Task<IActionResult> ProcessAllProductWithProcess([FromQuery] string? tableName, CancellationToken cancellationToken)
    {
        var safeTableName = NormalizeTableName(tableName);

        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        await EnsureAllProductTableAsync(connection, safeTableName, cancellationToken);
        await EnsureProductManagementTablesAsync(connection, cancellationToken);

        var rows = new List<AllProductRow>();
        await using (var select = connection.CreateCommand())
        {
            select.CommandText = $@"
SELECT
    Id,
    SourceRowHash,
    MainProductName,
    MainProductSlug,
    GroupName,
    CategoryName,
    CategorySlug,
    ProductName,
    ProductEnglishName,
    ProductSlug,
    SKU,
    BrandName,
    PackageName,
    UnitWeight,
    PacksPerCarton,
    CartonQuantity,
    SalePriceCash,
    SalePriceInstallment,
    PurchasePriceCash,
    PurchasePriceInstallment,
    ShortDescription,
    FullDescription,
    ImageUrl,
    GalleryJson,
    Status,
    WooProductId,
    HaveOtherPackage,
    PackageOne,
    RawJson
FROM `{safeTableName}`
ORDER BY Id;";

            await using var reader = await select.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(AllProductRow.From(reader));
            }
        }

        var groups = 0;
        var products = 0;
        var inactive = 0;

        foreach (var row in rows)
        {
            var mainName = FirstNotEmpty(row.MainProductName, row.CategoryName, row.GroupName, "بدون گروه");
            var mainSlug = FirstNotEmpty(row.MainProductSlug, row.CategorySlug, Slugify(mainName));

            var groupId = await UpsertMainGroupAsync(connection, mainName, mainSlug, row.CategoryName, row.CategorySlug, cancellationToken);
            groups++;

            var status = NormalizeStatus(row.Status, row.SalePriceCash, row.SalePriceInstallment);
            if (!string.Equals(status, "publish", StringComparison.OrdinalIgnoreCase))
            {
                inactive++;
            }

            await UpsertSaleProductAsync(connection, groupId, row, status, cancellationToken);
            products++;
        }

        return Ok(new
        {
            tableName = safeTableName,
            sourceRows = rows.Count,
            groupsTouched = groups,
            saleProductsTouched = products,
            inactiveProducts = inactive
        });
    }

    [HttpGet("source-table-summary")]
    public async Task<IActionResult> SourceTableSummary([FromQuery] string? tableName, CancellationToken cancellationToken)
    {
        var safeTableName = NormalizeTableName(tableName);
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await EnsureAllProductTableAsync(connection, safeTableName, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT
    COUNT(*) AS TotalRows,
    COUNT(DISTINCT MainProductName) AS MainProducts,
    COUNT(DISTINCT ProductName) AS ProductNames,
    SUM(CASE WHEN IFNULL(Status, '') IN ('draft','private','disabled') THEN 1 ELSE 0 END) AS DisabledRows
FROM `{safeTableName}`;";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return Ok(new { totalRows = 0 });
        }

        return Ok(new
        {
            tableName = safeTableName,
            totalRows = reader.GetInt64(0),
            mainProducts = reader.IsDBNull(1) ? 0 : reader.GetInt64(1),
            productNames = reader.IsDBNull(2) ? 0 : reader.GetInt64(2),
            disabledRows = reader.IsDBNull(3) ? 0 : reader.GetInt64(3)
        });
    }

    [HttpGet("sale-products")]
    public async Task<IActionResult> GetSaleProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 500);
        var offset = (page - 1) * pageSize;

        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await EnsureProductManagementTablesAsync(connection, cancellationToken);

        var where = string.IsNullOrWhiteSpace(search)
            ? ""
            : "WHERE p.ProductName LIKE @Search OR p.SKU LIKE @Search OR p.ProductSlug LIKE @Search OR g.MainProductName LIKE @Search";

        long total;
        await using (var count = connection.CreateCommand())
        {
            count.CommandText = $@"
SELECT COUNT(*)
FROM khb_sale_products p
LEFT JOIN khb_product_main_groups g ON g.Id = p.MainGroupId
{where};";
            if (!string.IsNullOrWhiteSpace(search)) Add(count, "@Search", $"%{search}%");
            total = Convert.ToInt64(await count.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }

        var items = new List<object>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $@"
SELECT
    p.Id,
    p.WooProductId,
    p.ProductName,
    p.SKU,
    p.ProductSlug,
    p.PackageName,
    p.CartonQuantity,
    p.SalePriceCash,
    p.SalePriceInstallment,
    p.PurchasePriceCash,
    p.PurchasePriceInstallment,
    p.Status,
    p.ImageUrl,
    p.UpdatedAtUtc,
    g.MainProductName
FROM khb_sale_products p
LEFT JOIN khb_product_main_groups g ON g.Id = p.MainGroupId
{where}
ORDER BY p.Id DESC
LIMIT @Take OFFSET @Skip;";
            if (!string.IsNullOrWhiteSpace(search)) Add(command, "@Search", $"%{search}%");
            Add(command, "@Take", pageSize);
            Add(command, "@Skip", offset);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new
                {
                    id = reader.GetInt64(0),
                    wooProductId = ReadNullableLong(reader, 1),
                    productName = ReadString(reader, 2),
                    sku = ReadString(reader, 3),
                    productSlug = ReadString(reader, 4),
                    packageName = ReadString(reader, 5),
                    cartonQuantity = ReadNullableInt(reader, 6),
                    salePriceCash = ReadNullableDecimal(reader, 7),
                    salePriceInstallment = ReadNullableDecimal(reader, 8),
                    purchasePriceCash = ReadNullableDecimal(reader, 9),
                    purchasePriceInstallment = ReadNullableDecimal(reader, 10),
                    status = ReadString(reader, 11),
                    imageUrl = ReadString(reader, 12),
                    updatedAtUtc = reader.IsDBNull(13) ? null : reader.GetDateTime(13).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    mainProductName = ReadString(reader, 14)
                });
            }
        }

        return Ok(new { page, pageSize, total, items });
    }

    private async Task<long> UpsertMainGroupAsync(MySqlConnection connection, string mainName, string mainSlug, string? categoryName, string? categorySlug, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO khb_product_main_groups
(MainProductName, MainProductSlug, CategoryName, CategorySlug, CreatedAtUtc, UpdatedAtUtc)
VALUES (@MainProductName, @MainProductSlug, @CategoryName, @CategorySlug, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE
    MainProductName = VALUES(MainProductName),
    CategoryName = VALUES(CategoryName),
    CategorySlug = VALUES(CategorySlug),
    UpdatedAtUtc = UTC_TIMESTAMP(6);
SELECT Id FROM khb_product_main_groups WHERE MainProductSlug = @MainProductSlug LIMIT 1;";
        Add(command, "@MainProductName", mainName);
        Add(command, "@MainProductSlug", mainSlug);
        Add(command, "@CategoryName", categoryName);
        Add(command, "@CategorySlug", categorySlug);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }

    private static async Task UpsertSaleProductAsync(MySqlConnection connection, long groupId, AllProductRow row, string status, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO khb_sale_products
(
    MainGroupId,
    SourceRowHash,
    WooProductId,
    ProductName,
    ProductEnglishName,
    ProductSlug,
    SKU,
    BrandName,
    PackageName,
    UnitWeight,
    PacksPerCarton,
    CartonQuantity,
    SalePriceCash,
    SalePriceInstallment,
    PurchasePriceCash,
    PurchasePriceInstallment,
    ShortDescription,
    FullDescription,
    ImageUrl,
    GalleryJson,
    Status,
    RawJson,
    CreatedAtUtc,
    UpdatedAtUtc
)
VALUES
(
    @MainGroupId,
    @SourceRowHash,
    @WooProductId,
    @ProductName,
    @ProductEnglishName,
    @ProductSlug,
    @SKU,
    @BrandName,
    @PackageName,
    @UnitWeight,
    @PacksPerCarton,
    @CartonQuantity,
    @SalePriceCash,
    @SalePriceInstallment,
    @PurchasePriceCash,
    @PurchasePriceInstallment,
    @ShortDescription,
    @FullDescription,
    @ImageUrl,
    @GalleryJson,
    @Status,
    @RawJson,
    UTC_TIMESTAMP(6),
    UTC_TIMESTAMP(6)
)
ON DUPLICATE KEY UPDATE
    MainGroupId = VALUES(MainGroupId),
    WooProductId = VALUES(WooProductId),
    ProductName = VALUES(ProductName),
    ProductEnglishName = VALUES(ProductEnglishName),
    ProductSlug = VALUES(ProductSlug),
    SKU = VALUES(SKU),
    BrandName = VALUES(BrandName),
    PackageName = VALUES(PackageName),
    UnitWeight = VALUES(UnitWeight),
    PacksPerCarton = VALUES(PacksPerCarton),
    CartonQuantity = VALUES(CartonQuantity),
    SalePriceCash = VALUES(SalePriceCash),
    SalePriceInstallment = VALUES(SalePriceInstallment),
    PurchasePriceCash = VALUES(PurchasePriceCash),
    PurchasePriceInstallment = VALUES(PurchasePriceInstallment),
    ShortDescription = VALUES(ShortDescription),
    FullDescription = VALUES(FullDescription),
    ImageUrl = VALUES(ImageUrl),
    GalleryJson = VALUES(GalleryJson),
    Status = VALUES(Status),
    RawJson = VALUES(RawJson),
    UpdatedAtUtc = UTC_TIMESTAMP(6);";

        Add(command, "@MainGroupId", groupId);
        Add(command, "@SourceRowHash", row.SourceRowHash);
        Add(command, "@WooProductId", row.WooProductId);
        Add(command, "@ProductName", row.ProductName);
        Add(command, "@ProductEnglishName", row.ProductEnglishName);
        Add(command, "@ProductSlug", FirstNotEmpty(row.ProductSlug, Slugify(row.ProductEnglishName), Slugify(row.ProductName)));
        Add(command, "@SKU", row.Sku);
        Add(command, "@BrandName", row.BrandName);
        Add(command, "@PackageName", row.PackageName);
        Add(command, "@UnitWeight", row.UnitWeight);
        Add(command, "@PacksPerCarton", row.PacksPerCarton);
        Add(command, "@CartonQuantity", row.CartonQuantity);
        Add(command, "@SalePriceCash", row.SalePriceCash);
        Add(command, "@SalePriceInstallment", row.SalePriceInstallment);
        Add(command, "@PurchasePriceCash", row.PurchasePriceCash);
        Add(command, "@PurchasePriceInstallment", row.PurchasePriceInstallment);
        Add(command, "@ShortDescription", row.ShortDescription);
        Add(command, "@FullDescription", row.FullDescription);
        Add(command, "@ImageUrl", row.ImageUrl);
        Add(command, "@GalleryJson", row.GalleryJson);
        Add(command, "@Status", status);
        Add(command, "@RawJson", row.RawJson);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureAllProductTableAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        await ExecuteAsync(connection, $@"
CREATE TABLE IF NOT EXISTS `{tableName}` (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    ImportBatchId VARCHAR(64) NULL,
    SourceRowNumber INT NULL,
    SourceRowHash CHAR(64) NOT NULL,
    RawJson LONGTEXT NULL,
    MainProductName VARCHAR(500) NULL,
    MainProductSlug VARCHAR(500) NULL,
    GroupName VARCHAR(500) NULL,
    CategoryName VARCHAR(500) NULL,
    CategorySlug VARCHAR(500) NULL,
    ProductName VARCHAR(700) NULL,
    ProductEnglishName VARCHAR(700) NULL,
    ProductSlug VARCHAR(700) NULL,
    SKU VARCHAR(191) NULL,
    BrandName VARCHAR(300) NULL,
    PackageName VARCHAR(300) NULL,
    UnitWeight DECIMAL(18,6) NULL,
    PacksPerCarton INT NULL,
    CartonQuantity INT NULL,
    SalePriceCash DECIMAL(18,2) NULL,
    SalePriceInstallment DECIMAL(18,2) NULL,
    PurchasePriceCash DECIMAL(18,2) NULL,
    PurchasePriceInstallment DECIMAL(18,2) NULL,
    ShortDescription LONGTEXT NULL,
    FullDescription LONGTEXT NULL,
    ImageUrl LONGTEXT NULL,
    GalleryJson LONGTEXT NULL,
    Status VARCHAR(100) NULL,
    WooProductId BIGINT NULL,
    HaveOtherPackage TINYINT(1) NULL,
    PackageOne VARCHAR(300) NULL,
    CreatedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY UX_All_Product_With_Process_SourceRowHash (SourceRowHash),
    KEY IX_All_Product_With_Process_ProductName (ProductName(191)),
    KEY IX_All_Product_With_Process_MainProductName (MainProductName(191)),
    KEY IX_All_Product_With_Process_SKU (SKU)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;", cancellationToken);
    }

    private static async Task EnsureProductManagementTablesAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        await ExecuteAsync(connection, @"
CREATE TABLE IF NOT EXISTS khb_product_main_groups (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    MainProductName VARCHAR(500) NOT NULL,
    MainProductSlug VARCHAR(500) NOT NULL,
    CategoryName VARCHAR(500) NULL,
    CategorySlug VARCHAR(500) NULL,
    Description LONGTEXT NULL,
    ImageUrl LONGTEXT NULL,
    CreatedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY UX_khb_product_main_groups_slug (MainProductSlug)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;", cancellationToken);

        await ExecuteAsync(connection, @"
CREATE TABLE IF NOT EXISTS khb_sale_products (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    MainGroupId BIGINT NOT NULL,
    SourceRowHash CHAR(64) NOT NULL,
    WooProductId BIGINT NULL,
    ProductName VARCHAR(700) NOT NULL,
    ProductEnglishName VARCHAR(700) NULL,
    ProductSlug VARCHAR(700) NULL,
    SKU VARCHAR(191) NULL,
    BrandName VARCHAR(300) NULL,
    PackageName VARCHAR(300) NULL,
    UnitWeight DECIMAL(18,6) NULL,
    PacksPerCarton INT NULL,
    CartonQuantity INT NULL,
    SalePriceCash DECIMAL(18,2) NULL,
    SalePriceInstallment DECIMAL(18,2) NULL,
    PurchasePriceCash DECIMAL(18,2) NULL,
    PurchasePriceInstallment DECIMAL(18,2) NULL,
    ShortDescription LONGTEXT NULL,
    FullDescription LONGTEXT NULL,
    ImageUrl LONGTEXT NULL,
    GalleryJson LONGTEXT NULL,
    Status VARCHAR(100) NOT NULL DEFAULT 'draft',
    RawJson LONGTEXT NULL,
    CreatedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY UX_khb_sale_products_hash (SourceRowHash),
    KEY IX_khb_sale_products_woo (WooProductId),
    KEY IX_khb_sale_products_sku (SKU),
    KEY IX_khb_sale_products_name (ProductName(191)),
    CONSTRAINT FK_khb_sale_products_group FOREIGN KEY (MainGroupId) REFERENCES khb_product_main_groups(Id)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;", cancellationToken);
    }

    private static async Task ExecuteAsync(MySqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string NormalizeTableName(string? tableName)
    {
        var value = string.IsNullOrWhiteSpace(tableName) ? DefaultTableName : tableName.Trim();
        if (!value.All(ch => char.IsLetterOrDigit(ch) || ch == '_'))
        {
            throw new ArgumentException("Table name may contain only letters, numbers and underscore.");
        }
        return value;
    }

    private string GetConnectionString()
    {
        var connectionString = _configuration.GetConnectionString("MySqlConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:MySqlConnection is not configured.");
        }
        return connectionString.Replace("SslMode=None", "SslMode=Disabled", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string> ReadCsvAsUtf8Async(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static void Add(MySqlCommand command, string name, object? value)
    {
        command.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    private static string ReadString(System.Data.Common.DbDataReader reader, int index)
        => reader.IsDBNull(index) ? string.Empty : reader.GetString(index);

    private static int? ReadNullableInt(System.Data.Common.DbDataReader reader, int index)
        => reader.IsDBNull(index) ? null : reader.GetInt32(index);

    private static long? ReadNullableLong(System.Data.Common.DbDataReader reader, int index)
        => reader.IsDBNull(index) ? null : reader.GetInt64(index);

    private static decimal? ReadNullableDecimal(System.Data.Common.DbDataReader reader, int index)
        => reader.IsDBNull(index) ? null : reader.GetDecimal(index);

    private static string FirstNotEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? string.Empty;

    private static string NormalizeStatus(string? status, decimal? cash, decimal? installment)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return cash.GetValueOrDefault() > 0 || installment.GetValueOrDefault() > 0 ? "publish" : "draft";
        }

        var value = status.Trim().ToLowerInvariant();
        return value switch
        {
            "1" or "true" or "فعال" or "publish" or "published" => "publish",
            "0" or "false" or "غیرفعال" or "draft" or "disabled" => "draft",
            _ => value
        };
    }

    private static string Slugify(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var builder = new StringBuilder();
        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch)) builder.Append(ch);
            else if (ch is ' ' or '-' or '_' or '/' or '\\') builder.Append('-');
        }
        var slug = builder.ToString();
        while (slug.Contains("--", StringComparison.Ordinal)) slug = slug.Replace("--", "-", StringComparison.Ordinal);
        return slug.Trim('-');
    }

    private sealed record ProductMappedRow(
        string? MainProductName,
        string? MainProductSlug,
        string? GroupName,
        string? CategoryName,
        string? CategorySlug,
        string ProductName,
        string? ProductEnglishName,
        string? ProductSlug,
        string? Sku,
        string? BrandName,
        string? PackageName,
        decimal? UnitWeight,
        int? PacksPerCarton,
        int? CartonQuantity,
        decimal? SalePriceCash,
        decimal? SalePriceInstallment,
        decimal? PurchasePriceCash,
        decimal? PurchasePriceInstallment,
        string? ShortDescription,
        string? FullDescription,
        string? ImageUrl,
        string? GalleryJson,
        string? Status,
        long? WooProductId,
        int? HaveOtherPackage,
        string? PackageOne);

    private sealed record AllProductRow(
        long Id,
        string SourceRowHash,
        string? MainProductName,
        string? MainProductSlug,
        string? GroupName,
        string? CategoryName,
        string? CategorySlug,
        string ProductName,
        string? ProductEnglishName,
        string? ProductSlug,
        string? Sku,
        string? BrandName,
        string? PackageName,
        decimal? UnitWeight,
        int? PacksPerCarton,
        int? CartonQuantity,
        decimal? SalePriceCash,
        decimal? SalePriceInstallment,
        decimal? PurchasePriceCash,
        decimal? PurchasePriceInstallment,
        string? ShortDescription,
        string? FullDescription,
        string? ImageUrl,
        string? GalleryJson,
        string? Status,
        long? WooProductId,
        int? HaveOtherPackage,
        string? PackageOne,
        string? RawJson)
    {
        public static AllProductRow From(System.Data.Common.DbDataReader r) => new(
            r.GetInt64(0),
            ReadString(r, 1),
            ReadString(r, 2),
            ReadString(r, 3),
            ReadString(r, 4),
            ReadString(r, 5),
            ReadString(r, 6),
            ReadString(r, 7),
            ReadString(r, 8),
            ReadString(r, 9),
            ReadString(r, 10),
            ReadString(r, 11),
            ReadString(r, 12),
            ReadNullableDecimal(r, 13),
            ReadNullableInt(r, 14),
            ReadNullableInt(r, 15),
            ReadNullableDecimal(r, 16),
            ReadNullableDecimal(r, 17),
            ReadNullableDecimal(r, 18),
            ReadNullableDecimal(r, 19),
            ReadString(r, 20),
            ReadString(r, 21),
            ReadString(r, 22),
            ReadString(r, 23),
            ReadString(r, 24),
            ReadNullableLong(r, 25),
            ReadNullableInt(r, 26),
            ReadString(r, 27),
            ReadString(r, 28));
    }

    private static class ProductRowMapper
    {
        public static ProductMappedRow Map(IReadOnlyDictionary<string, string> row)
        {
            var productName = Get(row, "ProductName", "Name", "Title", "PersianName", "FaName", "نام کالا", "نام محصول", "نام", "کالا", "محصول");
            var englishName = Get(row, "ProductEnglishName", "EnglishName", "EnName", "NameEn", "نام انگلیسی", "English Name");
            var category = Get(row, "CategoryName", "Category", "En_Taxonomic", "Taxonomic", "دسته", "دسته بندی", "دسته‌بندی", "گروه اصلی");
            var main = Get(row, "MainProductName", "MainProduct", "Commodity", "Family", "محصول اصلی", "کالای اصلی", "گروه محصول");
            var package = Get(row, "PackageName", "Packaging", "Package", "Package_One", "نوع بسته بندی", "بسته بندی", "بسته‌بندی");
            var sku = Get(row, "SKU", "Sku", "Code", "ProductCode", "کد کالا", "کد محصول");
            var slug = Get(row, "Slug", "ProductSlug", "slug");

            if (string.IsNullOrWhiteSpace(productName))
            {
                productName = FirstNotEmpty(englishName, sku);
            }

            var saleCash = GetDecimal(row, "SalePriceCash", "CashSalePrice", "cash_sale_price", "regular_price", "RegularPrice", "قیمت فروش نقد", "فروش نقد", "قیمت نقد");
            var saleInstallment = GetDecimal(row, "SalePriceInstallment", "InstallmentSalePrice", "installment_sale_price", "شرایطی", "قیمت فروش شرایطی", "فروش شرایطی");
            var buyCash = GetDecimal(row, "PurchasePriceCash", "CashPurchasePrice", "قیمت خرید نقد", "خرید نقد");
            var buyInstallment = GetDecimal(row, "PurchasePriceInstallment", "InstallmentPurchasePrice", "قیمت خرید شرایطی", "خرید شرایطی");

            return new ProductMappedRow(
                MainProductName: FirstNotEmpty(main, category),
                MainProductSlug: Get(row, "MainProductSlug", "CategorySlug", "En_Taxonomic", "TaxonomicSlug"),
                GroupName: Get(row, "GroupName", "Group", "گروه"),
                CategoryName: category,
                CategorySlug: Get(row, "CategorySlug", "En_Taxonomic", "TaxonomicSlug"),
                ProductName: productName,
                ProductEnglishName: englishName,
                ProductSlug: slug,
                Sku: sku,
                BrandName: Get(row, "Brand", "BrandName", "برند"),
                PackageName: package,
                UnitWeight: GetDecimal(row, "UnitWeight", "Weight", "وزن", "وزن واحد"),
                PacksPerCarton: GetInt(row, "PacksPerCarton", "QuantityInBox", "CartonQuantity", "تعداد در کارتن", "تعداد کارتن"),
                CartonQuantity: GetInt(row, "CartonQuantity", "QuantityInBox", "PacksPerCarton", "تعداد در کارتن", "تعداد داخل کارتن"),
                SalePriceCash: saleCash,
                SalePriceInstallment: saleInstallment,
                PurchasePriceCash: buyCash,
                PurchasePriceInstallment: buyInstallment,
                ShortDescription: Get(row, "ShortDescription", "short_description", "توضیحات کوتاه"),
                FullDescription: Get(row, "Description", "FullDescription", "description", "توضیحات", "توضیحات کلی"),
                ImageUrl: Get(row, "ImageUrl", "Image", "Images", "تصویر", "عکس محصول"),
                GalleryJson: Get(row, "GalleryJson", "Gallery", "گالری"),
                Status: Get(row, "Status", "وضعیت", "فعال"),
                WooProductId: GetLong(row, "WooProductId", "WordPressId", "ID", "id", "شناسه ووکامرس"),
                HaveOtherPackage: GetInt(row, "Have_Other_Packege", "HaveOtherPackage", "بسته بندی دیگر"),
                PackageOne: Get(row, "Package_One", "PackageOne", "بسته اول"));
        }

        private static string Get(IReadOnlyDictionary<string, string> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                var match = row.FirstOrDefault(x => NormalizeHeader(x.Key) == NormalizeHeader(key));
                if (!string.IsNullOrWhiteSpace(match.Value)) return match.Value.Trim();
            }
            return string.Empty;
        }

        private static int? GetInt(IReadOnlyDictionary<string, string> row, params string[] keys)
        {
            var text = Get(row, keys);
            var dec = ParseDecimal(text);
            return dec.HasValue ? Convert.ToInt32(Math.Round(dec.Value, 0), CultureInfo.InvariantCulture) : null;
        }

        private static long? GetLong(IReadOnlyDictionary<string, string> row, params string[] keys)
        {
            var text = Get(row, keys);
            var dec = ParseDecimal(text);
            return dec.HasValue ? Convert.ToInt64(Math.Round(dec.Value, 0), CultureInfo.InvariantCulture) : null;
        }

        private static decimal? GetDecimal(IReadOnlyDictionary<string, string> row, params string[] keys)
            => ParseDecimal(Get(row, keys));

        private static decimal? ParseDecimal(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var cleaned = NormalizeDigits(text)
                .Replace("تومان", "", StringComparison.OrdinalIgnoreCase)
                .Replace("ریال", "", StringComparison.OrdinalIgnoreCase)
                .Replace(",", "", StringComparison.Ordinal)
                .Replace(" ", "", StringComparison.Ordinal)
                .Trim();
            return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;
        }

        private static string NormalizeHeader(string value)
            => NormalizeDigits(value).Replace(" ", "", StringComparison.Ordinal).Replace("_", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal).Trim().ToLowerInvariant();

        private static string NormalizeDigits(string text)
        {
            var map = new Dictionary<char, char>
            {
                ['۰'] = '0', ['۱'] = '1', ['۲'] = '2', ['۳'] = '3', ['۴'] = '4', ['۵'] = '5', ['۶'] = '6', ['۷'] = '7', ['۸'] = '8', ['۹'] = '9',
                ['٠'] = '0', ['١'] = '1', ['٢'] = '2', ['٣'] = '3', ['٤'] = '4', ['٥'] = '5', ['٦'] = '6', ['٧'] = '7', ['٨'] = '8', ['٩'] = '9'
            };
            var chars = text.Select(ch => map.TryGetValue(ch, out var replacement) ? replacement : ch).ToArray();
            return new string(chars);
        }
    }

    private sealed class CsvParser
    {
        public List<string> Headers { get; init; } = new();
        public List<List<string>> Rows { get; init; } = new();

        public static CsvParser Parse(string text)
        {
            text = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            var delimiter = DetectDelimiter(text);
            var records = ReadRecords(text, delimiter);
            var parser = new CsvParser();
            if (records.Count == 0) return parser;

            parser.Headers.AddRange(records[0].Select(h => h.Trim().Trim('\uFEFF')));
            foreach (var record in records.Skip(1))
            {
                if (record.All(string.IsNullOrWhiteSpace)) continue;
                while (record.Count < parser.Headers.Count) record.Add(string.Empty);
                parser.Rows.Add(record.Take(parser.Headers.Count).ToList());
            }
            return parser;
        }

        public static Dictionary<string, string> ToDictionary(IReadOnlyList<string> headers, IReadOnlyList<string> values)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Count; i++)
            {
                var key = string.IsNullOrWhiteSpace(headers[i]) ? $"Column{i + 1}" : headers[i].Trim();
                if (dictionary.ContainsKey(key)) key = $"{key}_{i + 1}";
                dictionary[key] = i < values.Count ? values[i] : string.Empty;
            }
            return dictionary;
        }

        private static char DetectDelimiter(string text)
        {
            var firstLine = text.Split('\n').FirstOrDefault() ?? string.Empty;
            var candidates = new[] { ',', ';', '\t', '|' };
            return candidates.OrderByDescending(c => firstLine.Count(ch => ch == c)).First();
        }

        private static List<List<string>> ReadRecords(string text, char delimiter)
        {
            var records = new List<List<string>>();
            var row = new List<string>();
            var cell = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                    continue;
                }

                if (!inQuotes && ch == delimiter)
                {
                    row.Add(cell.ToString());
                    cell.Clear();
                    continue;
                }

                if (!inQuotes && ch == '\n')
                {
                    row.Add(cell.ToString());
                    cell.Clear();
                    records.Add(row);
                    row = new List<string>();
                    continue;
                }

                cell.Append(ch);
            }

            row.Add(cell.ToString());
            if (row.Count > 1 || !string.IsNullOrWhiteSpace(row[0])) records.Add(row);
            return records;
        }
    }
}
