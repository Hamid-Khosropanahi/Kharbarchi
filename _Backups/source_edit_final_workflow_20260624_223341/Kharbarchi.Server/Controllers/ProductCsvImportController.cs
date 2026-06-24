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
    private const decimal DefaultRetailPackagingFeePerPack = 30000m;

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
    BrandEnglishName,
    PackageName,
    UnitWeight,
    PacksPerCarton,
    CartonQuantity,
    PackagingPricePerPack,
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
    @BrandEnglishName,
    @PackageName,
    @UnitWeight,
    @PacksPerCarton,
    @CartonQuantity,
    @PackagingPricePerPack,
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
    BrandEnglishName = VALUES(BrandEnglishName),
    PackageName = VALUES(PackageName),
    UnitWeight = VALUES(UnitWeight),
    PacksPerCarton = VALUES(PacksPerCarton),
    CartonQuantity = VALUES(CartonQuantity),
    PackagingPricePerPack = VALUES(PackagingPricePerPack),
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
            Add(command, "@BrandEnglishName", mapped.BrandEnglishName);
            Add(command, "@PackageName", mapped.PackageName);
            Add(command, "@UnitWeight", mapped.UnitWeight);
            Add(command, "@PacksPerCarton", mapped.PacksPerCarton);
            Add(command, "@CartonQuantity", mapped.CartonQuantity);
            Add(command, "@PackagingPricePerPack", mapped.PackagingPricePerPack);
            Add(command, "@SalePriceCash", mapped.SaleKgPriceCash);
            Add(command, "@SalePriceInstallment", mapped.SaleKgPriceInstallment);
            Add(command, "@PurchasePriceCash", mapped.PurchaseKgPriceCash);
            Add(command, "@PurchasePriceInstallment", mapped.PurchaseKgPriceInstallment);
            Add(command, "@ShortDescription", mapped.ShortDescription);
            Add(command, "@FullDescription", mapped.FullDescription);
            Add(command, "@ImageUrl", mapped.ImageUrl);
            Add(command, "@GalleryJson", mapped.GalleryJson);
            Add(command, "@Status", mapped.Status);
            Add(command, "@WooProductId", mapped.WooProductId);
            Add(command, "@HaveOtherPackage", mapped.HaveOtherPackage);
            Add(command, "@PackageOne", mapped.PackageOne);

            var affected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (affected == 1) inserted++; else updated++;
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
    BrandEnglishName,
    PackageName,
    UnitWeight,
    PacksPerCarton,
    CartonQuantity,
    PackagingPricePerPack,
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
        var retailProducts = 0;
        var bulkProducts = 0;

        foreach (var row in rows)
        {
            var mainName = FirstNotEmpty(row.MainProductName, row.CategoryName, row.GroupName, "بدون گروه");
            var mainSlug = FirstNotEmpty(row.MainProductSlug, row.CategorySlug, Slugify(mainName));

            var groupId = await UpsertMainGroupAsync(
    connection,
    mainName ?? string.Empty,
    mainSlug ?? string.Empty,
    row.CategoryName ?? string.Empty,
    row.CategorySlug ?? string.Empty,
    cancellationToken);
            groups++;

            foreach (var finalProduct in BuildFinalSaleProducts(row))
            {
                if (!string.Equals(finalProduct.Status, "publish", StringComparison.OrdinalIgnoreCase)) inactive++;
                if (string.Equals(finalProduct.PackagingGroup, "retail", StringComparison.OrdinalIgnoreCase)) retailProducts++; else bulkProducts++;

                await UpsertSaleProductAsync(connection, groupId, finalProduct, cancellationToken);
                products++;
            }
        }

        return Ok(new
        {
            tableName = safeTableName,
            sourceRows = rows.Count,
            groupsTouched = groups,
            saleProductsTouched = products,
            bulkProducts,
            retailProducts,
            inactiveProducts = inactive,
            pricingRule = "bulk = kg price * kg; retail = packs per carton * ((kg price * unit kg) + package fee per pack)"
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
        if (!await reader.ReadAsync(cancellationToken)) return Ok(new { totalRows = 0 });

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
    p.PackagingGroup,
    p.PackageCode,
    p.UnitWeight,
    p.CartonQuantity,
    p.PackagingPricePerPack,
    p.KgPriceCash,
    p.KgPriceInstallment,
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
                    packagingGroup = ReadString(reader, 6),
                    packageCode = ReadString(reader, 7),
                    unitWeight = ReadNullableDecimal(reader, 8),
                    cartonQuantity = ReadNullableInt(reader, 9),
                    packagingPricePerPack = ReadNullableDecimal(reader, 10),
                    kgPriceCash = ReadNullableDecimal(reader, 11),
                    kgPriceInstallment = ReadNullableDecimal(reader, 12),
                    salePriceCash = ReadNullableDecimal(reader, 13),
                    salePriceInstallment = ReadNullableDecimal(reader, 14),
                    purchasePriceCash = ReadNullableDecimal(reader, 15),
                    purchasePriceInstallment = ReadNullableDecimal(reader, 16),
                    status = ReadString(reader, 17),
                    imageUrl = ReadString(reader, 18),
                    updatedAtUtc = reader.IsDBNull(19) ? null : reader.GetDateTime(19).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    mainProductName = ReadString(reader, 20)
                });
            }
        }

        return Ok(new { page, pageSize, total, items });
    }

    private static IEnumerable<SaleProductDraft> BuildFinalSaleProducts(AllProductRow row)
    {
        var baseName = FirstNotEmpty(row.ProductName, row.ProductEnglishName, row.Sku, "product");
        var bulkWeight = row.UnitWeight ?? ExtractFirstDecimal(row.PackageName) ?? ExtractFirstDecimal(row.PackageOne) ?? 1m;
        var bulkPackageTitle = FirstNotEmpty(row.PackageName, row.PackageOne, $"{bulkWeight:0.###} kg");
        var bulkCode = BuildPackageCode(bulkPackageTitle, bulkWeight, "BULK");

        yield return BuildOne(
            row,
            productName: CombineProductName(baseName, bulkPackageTitle),
            packageName: bulkPackageTitle,
            packagingGroup: "bulk",
            packageCode: bulkCode,
            unitWeight: bulkWeight,
            cartonQuantity: 1,
            packagingPricePerPack: 0m,
            sourceSuffix: bulkCode);

        if (row.HaveOtherPackage == 1)
        {
            var fee = row.PackagingPricePerPack ?? DefaultRetailPackagingFeePerPack;

            yield return BuildOne(
                row,
                productName: CombineProductName(baseName, "450 گرمی کارتن 12 عددی"),
                packageName: "450 گرمی",
                packagingGroup: "retail",
                packageCode: "450",
                unitWeight: 0.45m,
                cartonQuantity: 12,
                packagingPricePerPack: fee,
                sourceSuffix: "450");

            yield return BuildOne(
                row,
                productName: CombineProductName(baseName, "900 گرمی کارتن 6 عددی"),
                packageName: "900 گرمی",
                packagingGroup: "retail",
                packageCode: "900",
                unitWeight: 0.90m,
                cartonQuantity: 6,
                packagingPricePerPack: fee,
                sourceSuffix: "900");
        }
    }

    private static SaleProductDraft BuildOne(
        AllProductRow row,
        string productName,
        string packageName,
        string packagingGroup,
        string packageCode,
        decimal unitWeight,
        int cartonQuantity,
        decimal packagingPricePerPack,
        string sourceSuffix)
    {
        var saleCash = CalculateFinalPrice(row.SaleKgPriceCash, unitWeight, cartonQuantity, packagingPricePerPack, packagingGroup);
        var saleCredit = CalculateFinalPrice(row.SaleKgPriceInstallment, unitWeight, cartonQuantity, packagingPricePerPack, packagingGroup);
        var buyCash = CalculateFinalPrice(row.PurchaseKgPriceCash, unitWeight, cartonQuantity, packagingPricePerPack, packagingGroup);
        var buyCredit = CalculateFinalPrice(row.PurchaseKgPriceInstallment, unitWeight, cartonQuantity, packagingPricePerPack, packagingGroup);
        var status = NormalizeStatus(row.Status, saleCash, saleCredit);
        var slug = FirstNotEmpty(row.ProductSlug, Slugify(productName));
        var sku = BuildSku(row, packageCode);
        var sourceHash = ComputeSha256(row.SourceRowHash + "|" + sourceSuffix);

        return new SaleProductDraft(
            SourceRowHash: sourceHash,
            WooProductId: row.WooProductId,
            ProductName: productName,
            ProductEnglishName: row.ProductEnglishName,
            ProductSlug: Slugify(slug + "-" + packageCode),
            Sku: sku,
            BrandName: row.BrandName,
            BrandEnglishName: row.BrandEnglishName,
            PackageName: packageName,
            PackagingGroup: packagingGroup,
            PackageCode: packageCode,
            UnitWeight: unitWeight,
            PacksPerCarton: cartonQuantity,
            CartonQuantity: cartonQuantity,
            PackagingPricePerPack: packagingPricePerPack,
            KgPriceCash: row.SaleKgPriceCash,
            KgPriceInstallment: row.SaleKgPriceInstallment,
            SalePriceCash: saleCash,
            SalePriceInstallment: saleCredit,
            PurchasePriceCash: buyCash,
            PurchasePriceInstallment: buyCredit,
            ShortDescription: row.ShortDescription,
            FullDescription: row.FullDescription,
            ImageUrl: row.ImageUrl,
            GalleryJson: row.GalleryJson,
            Status: status,
            RawJson: row.RawJson);
    }

    private static decimal? CalculateFinalPrice(decimal? kgPrice, decimal unitWeight, int cartonQuantity, decimal packagingPricePerPack, string packagingGroup)
    {
        if (!kgPrice.HasValue || kgPrice.Value <= 0) return null;

        if (string.Equals(packagingGroup, "retail", StringComparison.OrdinalIgnoreCase))
        {
            return Math.Round(cartonQuantity * ((kgPrice.Value * unitWeight) + packagingPricePerPack), 0);
        }

        return Math.Round(unitWeight * kgPrice.Value, 0);
    }

    private static async Task<long> UpsertMainGroupAsync(
    MySqlConnection connection,
    string mainName,
    string mainSlug,
    string categoryName,
    string categorySlug,
    CancellationToken cancellationToken)
    {
        mainName = string.IsNullOrWhiteSpace(mainName) ? "بدون گروه" : mainName.Trim();
        mainSlug = string.IsNullOrWhiteSpace(mainSlug) ? BuildKharbarchiSlug(mainName) : mainSlug.Trim();

        categoryName = string.IsNullOrWhiteSpace(categoryName) ? mainName : categoryName.Trim();
        categorySlug = string.IsNullOrWhiteSpace(categorySlug) ? BuildKharbarchiSlug(categoryName) : categorySlug.Trim();

        var sourceKey = $"main:{categorySlug}:{mainSlug}".ToLowerInvariant();

        await using var command = connection.CreateCommand();

        command.CommandText = @"
INSERT INTO khb_product_main_groups
(
    SourceKey,
    Name,
    MainProductName,
    MainProductSlug,
    CategoryName,
    EnTaxonomic,
    CreatedAtUtc,
    UpdatedAtUtc
)
VALUES
(
    @SourceKey,
    @Name,
    @MainProductName,
    @MainProductSlug,
    @CategoryName,
    @EnTaxonomic,
    UTC_TIMESTAMP(6),
    UTC_TIMESTAMP(6)
)
ON DUPLICATE KEY UPDATE
    Name = VALUES(Name),
    MainProductName = VALUES(MainProductName),
    MainProductSlug = VALUES(MainProductSlug),
    CategoryName = VALUES(CategoryName),
    EnTaxonomic = VALUES(EnTaxonomic),
    UpdatedAtUtc = UTC_TIMESTAMP(6),
    Id = LAST_INSERT_ID(Id);

SELECT LAST_INSERT_ID();";

        command.Parameters.Clear();
        command.Parameters.AddWithValue("@SourceKey", sourceKey);
        command.Parameters.AddWithValue("@Name", mainName);
        command.Parameters.AddWithValue("@MainProductName", mainName);
        command.Parameters.AddWithValue("@MainProductSlug", mainSlug);
        command.Parameters.AddWithValue("@CategoryName", categoryName);
        command.Parameters.AddWithValue("@EnTaxonomic", categorySlug);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result);
    }
    private static string BuildKharbarchiSlug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "item";
        }

        var normalized = value
            .Trim()
            .ToLowerInvariant()
            .Replace('ي', 'ی')
            .Replace('ك', 'ک');

        var builder = new System.Text.StringBuilder();

        foreach (var ch in normalized)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
            }
            else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_')
            {
                builder.Append('-');
            }
        }

        var slug = System.Text.RegularExpressions.Regex
            .Replace(builder.ToString(), "-{2,}", "-")
            .Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? "item" : slug;
    }
    private static async Task UpsertSaleProductAsync(MySqlConnection connection, long groupId, SaleProductDraft row, CancellationToken cancellationToken)
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
    BrandEnglishName,
    PackageName,
    PackagingGroup,
    PackageCode,
    UnitWeight,
    PacksPerCarton,
    CartonQuantity,
    PackagingPricePerPack,
    KgPriceCash,
    KgPriceInstallment,
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
    @BrandEnglishName,
    @PackageName,
    @PackagingGroup,
    @PackageCode,
    @UnitWeight,
    @PacksPerCarton,
    @CartonQuantity,
    @PackagingPricePerPack,
    @KgPriceCash,
    @KgPriceInstallment,
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
    BrandEnglishName = VALUES(BrandEnglishName),
    PackageName = VALUES(PackageName),
    PackagingGroup = VALUES(PackagingGroup),
    PackageCode = VALUES(PackageCode),
    UnitWeight = VALUES(UnitWeight),
    PacksPerCarton = VALUES(PacksPerCarton),
    CartonQuantity = VALUES(CartonQuantity),
    PackagingPricePerPack = VALUES(PackagingPricePerPack),
    KgPriceCash = VALUES(KgPriceCash),
    KgPriceInstallment = VALUES(KgPriceInstallment),
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
        Add(command, "@ProductSlug", row.ProductSlug);
        Add(command, "@SKU", row.Sku);
        Add(command, "@BrandName", row.BrandName);
        Add(command, "@BrandEnglishName", row.BrandEnglishName);
        Add(command, "@PackageName", row.PackageName);
        Add(command, "@PackagingGroup", row.PackagingGroup);
        Add(command, "@PackageCode", row.PackageCode);
        Add(command, "@UnitWeight", row.UnitWeight);
        Add(command, "@PacksPerCarton", row.PacksPerCarton);
        Add(command, "@CartonQuantity", row.CartonQuantity);
        Add(command, "@PackagingPricePerPack", row.PackagingPricePerPack);
        Add(command, "@KgPriceCash", row.KgPriceCash);
        Add(command, "@KgPriceInstallment", row.KgPriceInstallment);
        Add(command, "@SalePriceCash", row.SalePriceCash);
        Add(command, "@SalePriceInstallment", row.SalePriceInstallment);
        Add(command, "@PurchasePriceCash", row.PurchasePriceCash);
        Add(command, "@PurchasePriceInstallment", row.PurchasePriceInstallment);
        Add(command, "@ShortDescription", row.ShortDescription);
        Add(command, "@FullDescription", row.FullDescription);
        Add(command, "@ImageUrl", row.ImageUrl);
        Add(command, "@GalleryJson", row.GalleryJson);
        Add(command, "@Status", row.Status);
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
    BrandEnglishName VARCHAR(300) NULL,
    PackageName VARCHAR(300) NULL,
    UnitWeight DECIMAL(18,6) NULL,
    PacksPerCarton INT NULL,
    CartonQuantity INT NULL,
    PackagingPricePerPack DECIMAL(18,2) NULL,
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

        await EnsureAllProductColumnsAsync(connection, tableName, cancellationToken);
    }

    private static async Task EnsureProductManagementTablesAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        await ExecuteAsync(connection, @"
CREATE TABLE IF NOT EXISTS khb_product_main_groups (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    MainProductName VARCHAR(500) NULL,
    MainProductSlug VARCHAR(500) NULL,
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
    MainGroupId BIGINT NULL,
    SourceRowHash CHAR(64) NOT NULL,
    WooProductId BIGINT NULL,
    ProductName VARCHAR(700) NULL,
    ProductEnglishName VARCHAR(700) NULL,
    ProductSlug VARCHAR(700) NULL,
    SKU VARCHAR(191) NULL,
    BrandName VARCHAR(300) NULL,
    BrandEnglishName VARCHAR(300) NULL,
    PackageName VARCHAR(300) NULL,
    PackagingGroup VARCHAR(50) NULL,
    PackageCode VARCHAR(50) NULL,
    UnitWeight DECIMAL(18,6) NULL,
    PacksPerCarton INT NULL,
    CartonQuantity INT NULL,
    PackagingPricePerPack DECIMAL(18,2) NULL,
    KgPriceCash DECIMAL(18,2) NULL,
    KgPriceInstallment DECIMAL(18,2) NULL,
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
    KEY IX_khb_sale_products_name (ProductName(191))
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;", cancellationToken);

        await EnsureProductManagementColumnsAsync(connection, cancellationToken);
    }

    private static async Task EnsureAllProductColumnsAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        await EnsureColumnAsync(connection, tableName, "ImportBatchId", "VARCHAR(64) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "SourceRowNumber", "INT NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "SourceRowHash", "CHAR(64) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "RawJson", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "MainProductName", "VARCHAR(500) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "MainProductSlug", "VARCHAR(500) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "GroupName", "VARCHAR(500) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "CategoryName", "VARCHAR(500) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "CategorySlug", "VARCHAR(500) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "ProductName", "VARCHAR(700) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "ProductEnglishName", "VARCHAR(700) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "ProductSlug", "VARCHAR(700) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "SKU", "VARCHAR(191) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "BrandName", "VARCHAR(300) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "BrandEnglishName", "VARCHAR(300) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "PackageName", "VARCHAR(300) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "UnitWeight", "DECIMAL(18,6) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "PacksPerCarton", "INT NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "CartonQuantity", "INT NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "PackagingPricePerPack", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "SalePriceCash", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "SalePriceInstallment", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "PurchasePriceCash", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "PurchasePriceInstallment", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "ShortDescription", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "FullDescription", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "ImageUrl", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "GalleryJson", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "Status", "VARCHAR(100) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "WooProductId", "BIGINT NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "HaveOtherPackage", "TINYINT(1) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "PackageOne", "VARCHAR(300) NULL", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "CreatedAtUtc", "DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)", cancellationToken);
        await EnsureColumnAsync(connection, tableName, "UpdatedAtUtc", "DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)", cancellationToken);
    }

    private static async Task EnsureProductManagementColumnsAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        await EnsureColumnAsync(connection, "khb_product_main_groups", "MainProductName", "VARCHAR(500) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_product_main_groups", "MainProductSlug", "VARCHAR(500) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_product_main_groups", "CategoryName", "VARCHAR(500) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_product_main_groups", "CategorySlug", "VARCHAR(500) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_product_main_groups", "Description", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_product_main_groups", "ImageUrl", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_product_main_groups", "CreatedAtUtc", "DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)", cancellationToken);
        await EnsureColumnAsync(connection, "khb_product_main_groups", "UpdatedAtUtc", "DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)", cancellationToken);

        await EnsureColumnAsync(connection, "khb_sale_products", "MainGroupId", "BIGINT NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "SourceRowHash", "CHAR(64) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "WooProductId", "BIGINT NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "ProductName", "VARCHAR(700) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "ProductEnglishName", "VARCHAR(700) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "ProductSlug", "VARCHAR(700) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "SKU", "VARCHAR(191) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "BrandName", "VARCHAR(300) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "BrandEnglishName", "VARCHAR(300) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "PackageName", "VARCHAR(300) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "PackagingGroup", "VARCHAR(50) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "PackageCode", "VARCHAR(50) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "UnitWeight", "DECIMAL(18,6) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "PacksPerCarton", "INT NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "CartonQuantity", "INT NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "PackagingPricePerPack", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "KgPriceCash", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "KgPriceInstallment", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "SalePriceCash", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "SalePriceInstallment", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "PurchasePriceCash", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "PurchasePriceInstallment", "DECIMAL(18,2) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "ShortDescription", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "FullDescription", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "ImageUrl", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "GalleryJson", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "Status", "VARCHAR(100) NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "RawJson", "LONGTEXT NULL", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "CreatedAtUtc", "DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)", cancellationToken);
        await EnsureColumnAsync(connection, "khb_sale_products", "UpdatedAtUtc", "DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)", cancellationToken);

        await CopyColumnIfExistsAsync(connection, "khb_sale_products", "ProductSku", "SKU", cancellationToken);
        await CopyColumnIfExistsAsync(connection, "khb_sale_products", "Slug", "ProductSlug", cancellationToken);
        await CopyColumnIfExistsAsync(connection, "khb_product_main_groups", "Name", "MainProductName", cancellationToken);
        await CopyColumnIfExistsAsync(connection, "khb_product_main_groups", "Slug", "MainProductSlug", cancellationToken);
    }

    private static async Task EnsureColumnAsync(MySqlConnection connection, string tableName, string columnName, string columnDefinition, CancellationToken cancellationToken)
    {
        await using var check = connection.CreateCommand();
        check.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @TableName
  AND COLUMN_NAME = @ColumnName;";
        Add(check, "@TableName", tableName);
        Add(check, "@ColumnName", columnName);
        var exists = Convert.ToInt64(await check.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) > 0;
        if (exists) return;

        await using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {columnDefinition};";
        await alter.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task CopyColumnIfExistsAsync(MySqlConnection connection, string tableName, string sourceColumn, string targetColumn, CancellationToken cancellationToken)
    {
        await using var check = connection.CreateCommand();
        check.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @TableName
  AND COLUMN_NAME = @SourceColumn;";
        Add(check, "@TableName", tableName);
        Add(check, "@SourceColumn", sourceColumn);
        var sourceExists = Convert.ToInt64(await check.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) > 0;
        if (!sourceExists) return;

        await EnsureColumnAsync(connection, tableName, targetColumn, "LONGTEXT NULL", cancellationToken);
        await using var update = connection.CreateCommand();
        update.CommandText = $"UPDATE `{tableName}` SET `{targetColumn}` = NULLIF(TRIM(`{sourceColumn}`), '') WHERE (`{targetColumn}` IS NULL OR `{targetColumn}` = '');";
        await update.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ExecuteAsync(MySqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private string GetConnectionString()
    {
        var connectionString = _configuration.GetConnectionString("MySqlConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:MySqlConnection is not configured. Use User Secrets or Environment Variables.");
        }
        return connectionString;
    }

    private static void Add(MySqlCommand command, string name, object? value)
    {
        command.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    private static string NormalizeTableName(string? tableName)
    {
        var name = string.IsNullOrWhiteSpace(tableName) ? DefaultTableName : tableName.Trim();
        if (name.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '_')))
        {
            throw new ArgumentException("Table name can only contain letters, digits and underscore.");
        }
        return name;
    }

    private static async Task<string> ReadCsvAsUtf8Async(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static string NormalizeStatus(string? inputStatus, decimal? saleCash, decimal? saleCredit)
    {
        var status = (inputStatus ?? string.Empty).Trim().ToLowerInvariant();
        if (status is "publish" or "published" or "active" or "فعال") return "publish";
        if (status is "draft" or "private" or "disabled" or "inactive" or "غیرفعال") return "draft";
        return saleCredit.HasValue && saleCredit.Value > 0 ? "publish" : "draft";
    }

    private static string FirstNotEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
        }
        return string.Empty;
    }

    private static string CombineProductName(string productName, string packageTitle)
    {
        if (string.IsNullOrWhiteSpace(packageTitle)) return productName;
        if (productName.Contains(packageTitle, StringComparison.OrdinalIgnoreCase)) return productName;
        return $"{productName} - {packageTitle}";
    }

    private static string BuildPackageCode(string packageTitle, decimal weight, string fallback)
    {
        var normalized = ProductRowMapper.NormalizeDigitsPublic(packageTitle).ToLowerInvariant();
        if (normalized.Contains("450")) return "450";
        if (normalized.Contains("900")) return "900";
        if (weight > 0) return "B" + weight.ToString("0.###", CultureInfo.InvariantCulture).Replace(".", "_");
        return fallback;
    }

    private static string BuildSku(AllProductRow row, string packageCode)
    {
        var existing = FirstNotEmpty(row.Sku);
        if (!string.IsNullOrWhiteSpace(existing)) return (existing + "_" + packageCode).ToUpperInvariant();

        var p = Abbrev(row.ProductEnglishName, row.ProductName, 2);
        var c = Abbrev(row.CategorySlug, row.CategoryName, 2);
        var b = Abbrev(row.BrandEnglishName, row.BrandName, 2);
        return $"{p}_{c}_{b}_{packageCode}".ToUpperInvariant();
    }

    private static string Abbrev(string? english, string? fallback, int length)
    {
        var source = FirstNotEmpty(english, fallback, "XX");
        var ascii = new string(source.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (ascii.Length >= length) return ascii[..length];
        return ascii.PadRight(length, 'X');
    }

    private static decimal? ExtractFirstDecimal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var normalized = ProductRowMapper.NormalizeDigitsPublic(text);
        var sb = new StringBuilder();
        var started = false;
        foreach (var ch in normalized)
        {
            if (char.IsDigit(ch) || ch == '.')
            {
                sb.Append(ch);
                started = true;
            }
            else if (started)
            {
                break;
            }
        }
        return decimal.TryParse(sb.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private static string Slugify(string? value)
    {
        var text = ProductRowMapper.NormalizeDigitsPublic(value ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(text)) return Guid.NewGuid().ToString("N")[..8];
        var builder = new StringBuilder();
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch)) builder.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch is '_' or '-' or '/' or '\\') builder.Append('-');
        }
        var slug = builder.ToString().Trim('-');
        while (slug.Contains("--", StringComparison.Ordinal)) slug = slug.Replace("--", "-", StringComparison.Ordinal);
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N")[..8] : slug;
    }

    private static string ComputeSha256(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadString(System.Data.Common.DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? string.Empty : Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty;

    private static decimal? ReadNullableDecimal(System.Data.Common.DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : Convert.ToDecimal(reader.GetValue(ordinal), CultureInfo.InvariantCulture);

    private static int? ReadNullableInt(System.Data.Common.DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture);

    private static long? ReadNullableLong(System.Data.Common.DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : Convert.ToInt64(reader.GetValue(ordinal), CultureInfo.InvariantCulture);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

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
        string? BrandEnglishName,
        string? PackageName,
        decimal? UnitWeight,
        int? PacksPerCarton,
        int? CartonQuantity,
        decimal? PackagingPricePerPack,
        decimal? SaleKgPriceCash,
        decimal? SaleKgPriceInstallment,
        decimal? PurchaseKgPriceCash,
        decimal? PurchaseKgPriceInstallment,
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
        string? BrandEnglishName,
        string? PackageName,
        decimal? UnitWeight,
        int? PacksPerCarton,
        int? CartonQuantity,
        decimal? PackagingPricePerPack,
        decimal? SaleKgPriceCash,
        decimal? SaleKgPriceInstallment,
        decimal? PurchaseKgPriceCash,
        decimal? PurchaseKgPriceInstallment,
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
            ReadString(r, 13),
            ReadNullableDecimal(r, 14),
            ReadNullableInt(r, 15),
            ReadNullableInt(r, 16),
            ReadNullableDecimal(r, 17),
            ReadNullableDecimal(r, 18),
            ReadNullableDecimal(r, 19),
            ReadNullableDecimal(r, 20),
            ReadNullableDecimal(r, 21),
            ReadString(r, 22),
            ReadString(r, 23),
            ReadString(r, 24),
            ReadString(r, 25),
            ReadString(r, 26),
            ReadNullableLong(r, 27),
            ReadNullableInt(r, 28),
            ReadString(r, 29),
            ReadString(r, 30));
    }

    private sealed record SaleProductDraft(
        string SourceRowHash,
        long? WooProductId,
        string ProductName,
        string? ProductEnglishName,
        string ProductSlug,
        string Sku,
        string? BrandName,
        string? BrandEnglishName,
        string PackageName,
        string PackagingGroup,
        string PackageCode,
        decimal UnitWeight,
        int PacksPerCarton,
        int CartonQuantity,
        decimal PackagingPricePerPack,
        decimal? KgPriceCash,
        decimal? KgPriceInstallment,
        decimal? SalePriceCash,
        decimal? SalePriceInstallment,
        decimal? PurchasePriceCash,
        decimal? PurchasePriceInstallment,
        string? ShortDescription,
        string? FullDescription,
        string? ImageUrl,
        string? GalleryJson,
        string Status,
        string? RawJson);

    private static class ProductRowMapper
    {
        public static ProductMappedRow Map(IReadOnlyDictionary<string, string> row)
        {
            var productName = Get(row, "ProductName", "Product_Name", "Name", "Title", "PersianName", "FaName", "نام کالا", "نام محصول", "نام", "کالا", "محصول");
            var englishName = Get(row, "ProductEnglishName", "Product_EnglishName", "EnglishName", "EnName", "NameEn", "نام انگلیسی", "English Name");
            var categoryName = Get(row, "CategoryName", "Category", "Base_terms", "BaseTerms", "terms", "دسته", "دسته بندی", "دسته‌بندی", "گروه اصلی");
            var main = Get(row, "MainProductName", "MainProduct", "Commodity", "Family", "Base_terms", "BaseTerms", "terms", "محصول اصلی", "کالای اصلی", "گروه محصول");
            var package = Get(row, "PackageName", "Packaging", "Package", "Package_One", "PackageOne", "نوع بسته بندی", "بسته بندی", "بسته‌بندی");
            var sku = Get(row, "SKU", "Sku", "Code", "ProductCode", "کد کالا", "کد محصول");
            var slug = Get(row, "Slug", "ProductSlug", "Product_Slug", "slug");
            var taxonomicSlug = Get(row, "CategorySlug", "En_Taxonomic", "TaxonomicSlug", "EnTaxonomic");

            if (string.IsNullOrWhiteSpace(productName)) productName = FirstNotEmpty(englishName, sku);

            var kgCash = GetDecimal(row, "KgPriceCash", "SaleKgPriceCash", "cash_price", "Cash_Price", "cash_sale_price", "قیمت کیلو نقد", "قیمت نقد کیلویی", "قیمت فروش نقد", "فروش نقد", "قیمت نقد");
            var kgCredit = GetDecimal(row, "KgPriceInstallment", "SaleKgPriceInstallment", "credit_price", "Credit_Price", "installment_sale_price", "قیمت کیلو شرایطی", "قیمت شرایطی کیلویی", "قیمت فروش شرایطی", "فروش شرایطی");
            var buyKgCash = GetDecimal(row, "PurchaseKgPriceCash", "BuyKgPriceCash", "purchase_cash_price", "قیمت خرید نقد", "خرید نقد", "قیمت خرید نقد کیلویی");
            var buyKgCredit = GetDecimal(row, "PurchaseKgPriceInstallment", "BuyKgPriceInstallment", "purchase_credit_price", "قیمت خرید شرایطی", "خرید شرایطی", "قیمت خرید شرایطی کیلویی");
            var packagingFee = GetDecimal(row, "PackagingPricePerPack", "PackageFee", "PackagingFee", "Package_Price", "قیمت بسته بندی", "هزینه بسته بندی", "قیمت بسته‌بندی");

            return new ProductMappedRow(
                MainProductName: FirstNotEmpty(main, categoryName),
                MainProductSlug: FirstNotEmpty(taxonomicSlug, Slugify(FirstNotEmpty(main, categoryName))),
                GroupName: Get(row, "GroupName", "Group", "terms", "Base_terms", "گروه"),
                CategoryName: categoryName,
                CategorySlug: taxonomicSlug,
                ProductName: productName,
                ProductEnglishName: englishName,
                ProductSlug: slug,
                Sku: sku,
                BrandName: Get(row, "Brand", "BrandName", "برند"),
                BrandEnglishName: Get(row, "Brand_En", "BrandEn", "BrandEnglishName", "BrandEnglish", "برند انگلیسی"),
                PackageName: package,
                UnitWeight: GetDecimal(row, "UnitWeight", "Weight", "Pakage_One_kg", "Package_One_kg", "PackageOneKg", "وزن", "وزن واحد"),
                PacksPerCarton: GetInt(row, "PacksPerCarton", "QuantityInBox", "CartonQuantity", "تعداد در کارتن", "تعداد کارتن"),
                CartonQuantity: GetInt(row, "CartonQuantity", "QuantityInBox", "PacksPerCarton", "تعداد در کارتن", "تعداد داخل کارتن"),
                PackagingPricePerPack: packagingFee,
                SaleKgPriceCash: kgCash,
                SaleKgPriceInstallment: kgCredit,
                PurchaseKgPriceCash: buyKgCash,
                PurchaseKgPriceInstallment: buyKgCredit,
                ShortDescription: Get(row, "ShortDescription", "short_description", "خلاصه", "توضیحات کوتاه"),
                FullDescription: Get(row, "Desc", "Description", "FullDescription", "description", "توضیحات", "توضیحات کلی"),
                ImageUrl: Get(row, "ImageUrl", "Image", "Images", "تصویر", "عکس محصول"),
                GalleryJson: Get(row, "GalleryJson", "Gallery", "گالری"),
                Status: Get(row, "Status", "وضعیت", "فعال"),
                WooProductId: GetLong(row, "WooProductId", "WordPressId", "ID", "id", "شناسه ووکامرس"),
                HaveOtherPackage: GetInt(row, "Have_Other_Package", "Have_Other_Packege", "HaveOtherPackage", "بسته بندی دیگر"),
                PackageOne: Get(row, "Package_One", "PackageOne", "بسته اول"));
        }

        private static string Get(IReadOnlyDictionary<string, string> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                var normalizedKey = NormalizeHeader(key);
                foreach (var pair in row)
                {
                    if (NormalizeHeader(pair.Key) == normalizedKey && !string.IsNullOrWhiteSpace(pair.Value)) return pair.Value.Trim();
                }
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
                .Replace("٬", "", StringComparison.Ordinal)
                .Replace("\u00A0", "", StringComparison.Ordinal)
                .Replace(" ", "", StringComparison.Ordinal)
                .Trim();
            return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;
        }

        private static string NormalizeHeader(string value)
            => NormalizeDigits(value)
                .Replace("\u00A0", "", StringComparison.Ordinal)
                .Replace("\u200C", "", StringComparison.Ordinal)
                .Replace(" ", "", StringComparison.Ordinal)
                .Replace("_", "", StringComparison.Ordinal)
                .Replace("-", "", StringComparison.Ordinal)
                .Replace("‑", "", StringComparison.Ordinal)
                .Trim()
                .ToLowerInvariant();

        public static string NormalizeDigitsPublic(string text) => NormalizeDigits(text);

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
            parser.Headers.AddRange(records[0].Select(x => x.Trim('\uFEFF').Trim()));
            for (var i = 1; i < records.Count; i++)
            {
                if (records[i].All(string.IsNullOrWhiteSpace)) continue;
                parser.Rows.Add(records[i]);
            }
            return parser;
        }

        public static Dictionary<string, string> ToDictionary(IReadOnlyList<string> headers, IReadOnlyList<string> row)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                result[headers[i]] = value;
            }
            return result;
        }

        private static char DetectDelimiter(string text)
        {
            var firstLine = text.Split('\n').FirstOrDefault() ?? string.Empty;
            var comma = firstLine.Count(c => c == ',');
            var semicolon = firstLine.Count(c => c == ';');
            var tab = firstLine.Count(c => c == '\t');
            if (tab > comma && tab > semicolon) return '\t';
            return semicolon > comma ? ';' : ',';
        }

        private static List<List<string>> ReadRecords(string text, char delimiter)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
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
                    row.Add(field.ToString());
                    field.Clear();
                    continue;
                }

                if (!inQuotes && ch == '\n')
                {
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = new List<string>();
                    continue;
                }

                field.Append(ch);
            }

            row.Add(field.ToString());
            if (row.Count > 1 || !string.IsNullOrWhiteSpace(row[0])) rows.Add(row);
            return rows;
        }
    }
}
