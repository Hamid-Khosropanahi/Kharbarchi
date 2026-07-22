using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kharbarchi.Server.Models;
using Kharbarchi.Server.Security;
using Kharbarchi.Server.Services;
using Kharbarchi.Shared.Contracts.ProductWorkflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/product-csv")]
[Authorize(Policy = AuthorizationPolicyNames.ProductImportWrite)]
public sealed class ProductCsvImportController : ControllerBase
{
    [HttpGet("workflow-table")]
    public async Task<IActionResult> GetWorkflowTable(
    [FromQuery] string? kind,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    [FromQuery] string? search = null,
    CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 5, 100);
        var offset = (page - 1) * pageSize;

        var tableName = (kind ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "source" => "khb_source_product",
            "category" => "khb_category_map",
            "commodity" => "khb_commodity",
            "package" => "khb_package_type",
            "product" => "khb_product_final",
            "queue" => "khb_product_update_queue",
            _ => null
        };

        if (tableName is null)
        {
            return BadRequest(new { error = "Invalid workflow table kind." });
        }

        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        long total;
        await using (var count = connection.CreateCommand())
        {
            count.CommandText = $"SELECT COUNT(*) FROM `{tableName}`;";
            total = Convert.ToInt64(await count.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }

        var rows = new List<Dictionary<string, string?>>();

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT * FROM `{tableName}` ORDER BY `Id` DESC LIMIT @Take OFFSET @Skip;";
            command.Parameters.AddWithValue("@Take", pageSize);
            command.Parameters.AddWithValue("@Skip", offset);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, string?>();

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);

                    if (reader.IsDBNull(i))
                    {
                        row[name] = null;
                        continue;
                    }

                    var value = reader.GetValue(i);

                    row[name] = value switch
                    {
                        DateTime date => date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        decimal number => number.ToString("N0", CultureInfo.InvariantCulture),
                        double number => number.ToString("N0", CultureInfo.InvariantCulture),
                        float number => number.ToString("N0", CultureInfo.InvariantCulture),
                        long number => number.ToString("N0", CultureInfo.InvariantCulture),
                        int number => number.ToString("N0", CultureInfo.InvariantCulture),
                        _ => value.ToString()
                    };
                }

                rows.Add(row);
            }
        }

        return Ok(new
        {
            kind,
            tableName,
            page,
            pageSize,
            total,
            items = rows
        });
    }
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductCsvImportController> _logger;
    private readonly WorkflowJobService _jobs;
    private readonly WooCommerceProfileService _profiles;

    private const string DefaultTableName = "all_product_with_process";
    private const decimal DefaultRetailPackagingFeePerPack = 30000m;

    public ProductCsvImportController(
        IConfiguration configuration,
        ILogger<ProductCsvImportController> logger,
        WorkflowJobService jobs,
        WooCommerceProfileService profiles)
    {
        _configuration = configuration;
        _logger = logger;
        _jobs = jobs;
        _profiles = profiles;
    }

    [HttpPost("jobs/{type}/start")]
    public async Task<ActionResult<WorkflowJobDto>> StartWorkflowJob(
        string type,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _jobs.CreateAsync(type, User.Identity?.Name, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("jobs/{jobId:guid}")]
    public async Task<ActionResult<WorkflowJobDto>> GetWorkflowJob(Guid jobId, CancellationToken cancellationToken)
    {
        var result = await _jobs.GetAsync(jobId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("jobs/{jobId:guid}/logs")]
    public async Task<ActionResult<IReadOnlyList<WorkflowJobLogDto>>> GetWorkflowJobLogs(
        Guid jobId,
        [FromQuery] int take = 200,
        CancellationToken cancellationToken = default) =>
        Ok(await _jobs.GetLogsAsync(jobId, take, cancellationToken));

    [HttpGet("jobs/latest")]
    public async Task<ActionResult<WorkflowJobDto?>> GetLatestWorkflowJob(
        [FromQuery] string? type,
        CancellationToken cancellationToken) =>
        Ok(await _jobs.GetLatestAsync(type, cancellationToken));

    [HttpPost("import-all-product-with-process")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> ImportAllProductWithProcess(
        IFormFile file,
        [FromQuery] string? tableName,
        [FromQuery] bool truncate = false,
        [FromQuery] Guid? jobId = null,
        CancellationToken cancellationToken = default)
    {
        var safeTableName = NormalizeTableName(tableName);
        var job = await GetOrCreateJobAsync(jobId, "import", cancellationToken);

        if (file is null || file.Length == 0)
        {
            await _jobs.FailAsync(job, "Reading CSV file", new InvalidDataException("CSV file is required."), cancellationToken);
            return BadRequest(new { error = "CSV file is required." });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            await _jobs.FailAsync(job, "Reading CSV file", new InvalidDataException("Only CSV files are supported."), cancellationToken);
            return BadRequest(new { error = "Only CSV files are supported. Save Excel as CSV UTF-8." });
        }

        var csvText = await ReadCsvAsUtf8Async(file, cancellationToken);
        if (csvText.Contains('\uFFFD'))
        {
            await _jobs.FailAsync(job, "Reading CSV file", new InvalidDataException("CSV encoding is not valid UTF-8."), cancellationToken);
            return BadRequest(new
            {
                error = "CSV encoding is not valid UTF-8. In Excel use: Save As > CSV UTF-8 (Comma delimited)."
            });
        }

        var parsed = CsvParser.Parse(csvText);
        if (parsed.Headers.Count == 0)
        {
            await _jobs.FailAsync(job, "Reading CSV file", new InvalidDataException("CSV header row was not found."), cancellationToken);
            return BadRequest(new { error = "CSV header row was not found." });
        }

        if (parsed.Rows.Count == 0)
        {
            await _jobs.FailAsync(job, "Reading CSV file", new InvalidDataException("CSV has headers but no data rows."), cancellationToken);
            return BadRequest(new { error = "CSV has headers but no data rows." });
        }

        await _jobs.StartAsync(job, "Validating data", parsed.Rows.Count, "CSV validated; starting source-row upsert.", cancellationToken);

        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        try
        {
            await ValidateAllProductTableAsync(connection, safeTableName, cancellationToken);
            await ValidateProductManagementTablesAsync(connection, cancellationToken);
        }
        catch (Exception ex)
        {
            await _jobs.FailAsync(job, "Validating database schema", ex, cancellationToken);
            return Conflict(new { jobId = job.JobId, error = ex.Message });
        }

        var truncateWasIgnored = truncate;

        var batchId = Guid.NewGuid().ToString("N");
        var inserted = 0;
        var updated = 0;
        var skipped = 0;
        var warnings = new List<string>();
        if (truncateWasIgnored)
        {
            warnings.Add("KHB-SAFE: درخواست truncate نادیده گرفته شد. طبق قوانین پروژه، import فقط upsert انجام می‌دهد و داده‌های قبلی را پاک نمی‌کند.");
        }

        for (var i = 0; i < parsed.Rows.Count; i++)
        {
            var row = parsed.Rows[i];
            var rowDictionary = CsvParser.ToDictionary(parsed.Headers, row);
            var mapped = ProductRowMapper.Map(rowDictionary);

            if (string.IsNullOrWhiteSpace(mapped.ProductName))
            {
                skipped++;
                warnings.Add($"Row {i + 2}: product name is empty.");
                mapped = mapped with { Status = "error" };
                await _jobs.AddLogAsync(
                    job,
                    "Validating data",
                    "source-row",
                    (i + 2).ToString(CultureInfo.InvariantCulture),
                    mapped.Sku,
                    "error",
                    "Product name is empty. The row was stored with Error status and will not be queued for WooCommerce.",
                    null,
                    null,
                    null,
                    cancellationToken);
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

            if ((i + 1) % 10 == 0 || i + 1 == parsed.Rows.Count)
            {
                await _jobs.UpdateAsync(
                    job,
                    "Saving source rows",
                    i + 1,
                    parsed.Rows.Count,
                    inserted + updated - skipped,
                    skipped,
                    0,
                    skipped,
                    $"Saved {i + 1} of {parsed.Rows.Count} source rows.",
                    cancellationToken);
            }
        }

        job.SuccessCount = inserted + updated - skipped;
        job.ErrorCount = skipped;
        job.SkippedCount = skipped;
        await _jobs.CompleteAsync(job, $"CSV import completed. Inserted: {inserted}; updated: {updated}; errors: {skipped}.", cancellationToken);

        return Ok(new
        {
            jobId = job.JobId,
            tableName = safeTableName,
            batchId,
            file = file.FileName,
            headers = parsed.Headers.Count,
            rows = parsed.Rows.Count,
            inserted,
            updated,
            skipped,
            truncateWasIgnored,
            warnings = warnings.Take(20).ToArray()
        });
    }

    [HttpPost("process-all-product-with-process")]
    public async Task<IActionResult> ProcessAllProductWithProcess(
        [FromQuery] string? tableName,
        [FromQuery] string? priceUnit = "rial",
        [FromQuery] bool clearGeneratedBeforeProcess = false,
        [FromQuery] bool? clearTargets = null,
        [FromQuery] Guid? jobId = null,
        CancellationToken cancellationToken = default)
    {
        var safeTableName = NormalizeTableName(tableName);
        var normalizedPriceUnit = NormalizePriceUnit(priceUnit);
        var clearWasRequested = clearTargets ?? clearGeneratedBeforeProcess;
        var job = await GetOrCreateJobAsync(jobId, "process", cancellationToken);

        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        try
        {
            await ValidateAllProductTableAsync(connection, safeTableName, cancellationToken);
            await ValidateProductManagementTablesAsync(connection, cancellationToken);
        }
        catch (Exception ex)
        {
            await _jobs.FailAsync(job, "Validating database schema", ex, cancellationToken);
            return Conflict(new { jobId = job.JobId, error = ex.Message });
        }
        if (clearWasRequested)
        {
            _logger.LogWarning("KHB-SAFE: clearGeneratedBeforeProcess/clearTargets was requested but ignored. Product workflow tables are never truncated or dropped by processing.");
        }

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
        var cartonPackagedProducts = 0;
        var cartonWeightProducts = 0;
        var skipped = 0;
        var errors = 0;
        var drafts = new List<(AllProductRow Source, SaleProductDraft Product)>();

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.ProductName)
                || string.Equals(row.Status, "error", StringComparison.OrdinalIgnoreCase))
            {
                skipped++;
                continue;
            }

            foreach (var finalProduct in BuildFinalSaleProducts(row, normalizedPriceUnit))
            {
                drafts.Add((row, finalProduct));
                if (!string.Equals(finalProduct.Status, "publish", StringComparison.OrdinalIgnoreCase)) inactive++;
                if (string.Equals(finalProduct.PackagingGroup, "retail", StringComparison.OrdinalIgnoreCase)) cartonPackagedProducts++; else cartonWeightProducts++;
            }
        }

        await _jobs.StartAsync(job, "Saving source rows", drafts.Count, "Preparing canonical product workflow tables.", cancellationToken);

        var groupIds = new Dictionary<long, long>();
        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            if (string.IsNullOrWhiteSpace(row.ProductName)
                || string.Equals(row.Status, "error", StringComparison.OrdinalIgnoreCase))
            {
                await _jobs.AddLogAsync(job, "Validating data", "source-row", row.Id.ToString(CultureInfo.InvariantCulture), row.Sku, "skipped", "Source row is incomplete or has Error status.", null, null, null, cancellationToken);
                continue;
            }

            var productEnglishName = ResolveEnglishProductName(row.ProductEnglishName, row.ProductName, row.MainProductName, row.CategorySlug);
            var mainName = FirstNotEmpty(row.MainProductName, row.CategoryName, row.GroupName, "بدون گروه");
            var mainSlug = BuildEnglishSlug(FirstNotEmpty(productEnglishName, row.CategorySlug, row.MainProductSlug, row.Sku, mainName));
            var categoryName = FirstNotEmpty(row.CategoryName, mainName);
            var categorySlug = BuildEnglishSlug(FirstNotEmpty(row.CategorySlug, productEnglishName, row.BrandEnglishName, categoryName));

            try
            {
                var groupId = await UpsertMainGroupAsync(
                    connection,
                    mainName,
                    mainSlug,
                    categoryName,
                    categorySlug,
                    cancellationToken);
                groupIds[row.Id] = groupId;
                groups++;
                await UpsertSourceProductAsync(connection, row, productEnglishName, normalizedPriceUnit, cancellationToken);
            }
            catch (Exception ex)
            {
                errors++;
                await _jobs.AddLogAsync(job, "Saving source rows", "source-row", row.Id.ToString(CultureInfo.InvariantCulture), row.Sku, "error", ex.Message, null, null, null, cancellationToken);
            }

            var percent = rows.Count == 0 ? 15 : 5 + (int)Math.Round((index + 1) * 10d / rows.Count);
            await _jobs.SetPhaseAsync(job, "Saving source rows", percent, drafts.Count, 0, 0, errors, inactive, skipped, $"Saved source row {index + 1} of {rows.Count}.", cancellationToken);
        }

        var prepared = drafts
            .Select(x => new PreparedSaleProduct(
                x.Source,
                x.Product,
                groupIds.GetValueOrDefault(x.Source.Id)))
            .ToList();
        var blocked = prepared
            .Where(x => x.GroupId <= 0)
            .Select(x => x.Product.SourceRowHash)
            .ToHashSet(StringComparer.Ordinal);

        errors += await ExecutePreparedPhaseAsync(connection, job, prepared, blocked, "Saving categories", 15, 30, UpsertCategoryWorkflowAsync, cancellationToken);
        errors += await ExecutePreparedPhaseAsync(connection, job, prepared, blocked, "Saving base products", 30, 45, UpsertCommodityWorkflowAsync, cancellationToken);
        errors += await ExecutePreparedPhaseAsync(connection, job, prepared, blocked, "Saving packaging records", 45, 60, UpsertPackageWorkflowAsync, cancellationToken);
        errors += await ExecutePreparedPhaseAsync(connection, job, prepared, blocked, "Creating final products", 60, 85, UpsertFinalWorkflowAsync, cancellationToken);
        errors += await ExecutePreparedPhaseAsync(
            connection,
            job,
            prepared,
            blocked,
            "Building WooCommerce sync queue",
            85,
            99,
            (db, row, token) => QueueWorkflowAsync(db, row, job.JobId, token),
            cancellationToken);

        products = prepared.Count - blocked.Count;
        job.SuccessCount = products;
        job.ErrorCount = errors;
        job.DraftCount = inactive;
        job.SkippedCount = skipped;
        job.ProcessedItems = products;
        await _jobs.CompleteAsync(job, $"Processing completed. Ready: {products}; draft: {inactive}; skipped: {skipped}; errors: {errors}.", cancellationToken);

        return Ok(new
        {
            jobId = job.JobId,
            tableName = safeTableName,
            inputPriceUnit = normalizedPriceUnit,
            outputPriceUnit = "toman",
            generatedTablesWereCleared = false,
            clearRequestWasIgnored = clearWasRequested,
            sourceRows = rows.Count,
            groupsTouched = groups,
            saleProductsTouched = products,
            cartonWeightProducts,
            cartonPackagedProducts,
            inactiveProducts = inactive,
            skippedProducts = skipped,
            errorProducts = errors,
            pricingRule = "KHB-246: carton-weight products use kg price * carton weight. Carton-packaged products use package price * packs per carton. Kg price and packaging fee may be stored as informational meta but are not used in carton-packaged calculations."
        });
    }


    [HttpGet("source-rows")]
    public async Task<IActionResult> GetSourceRows([FromQuery] string? tableName, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var safeTableName = NormalizeTableName(tableName);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 5, 100);
        var offset = (page - 1) * pageSize;

        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await ValidateAllProductTableAsync(connection, safeTableName, cancellationToken);

        var where = string.IsNullOrWhiteSpace(search)
            ? string.Empty
            : "WHERE ProductName LIKE @Search OR ProductEnglishName LIKE @Search OR MainProductName LIKE @Search OR SKU LIKE @Search OR BrandName LIKE @Search";

        long total;
        await using (var count = connection.CreateCommand())
        {
            count.CommandText = $"SELECT COUNT(*) FROM `{safeTableName}` {where};";
            if (!string.IsNullOrWhiteSpace(search)) Add(count, "@Search", $"%{search}%");
            total = Convert.ToInt64(await count.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
        }

        var items = new List<SourceRowDto>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $@"
SELECT
    Id,
    SourceRowNumber,
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
    CartonQuantity,
    PackagingPricePerPack,
    SalePriceCash,
    SalePriceInstallment,
    PurchasePriceCash,
    PurchasePriceInstallment,
    ShortDescription,
    FullDescription,
    ImageUrl,
    Status,
    HaveOtherPackage,
    PackageOne,
    UpdatedAtUtc
FROM `{safeTableName}`
{where}
ORDER BY Id
LIMIT @Take OFFSET @Skip;";
            if (!string.IsNullOrWhiteSpace(search)) Add(command, "@Search", $"%{search}%");
            Add(command, "@Take", pageSize);
            Add(command, "@Skip", offset);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new SourceRowDto
                {
                    Id = reader.GetInt64(0),
                    SourceRowNumber = ReadNullableInt(reader, 1),
                    MainProductName = ReadString(reader, 2),
                    MainProductSlug = ReadString(reader, 3),
                    GroupName = ReadString(reader, 4),
                    CategoryName = ReadString(reader, 5),
                    CategorySlug = ReadString(reader, 6),
                    ProductName = ReadString(reader, 7),
                    ProductEnglishName = ReadString(reader, 8),
                    ProductSlug = ReadString(reader, 9),
                    Sku = ReadString(reader, 10),
                    BrandName = ReadString(reader, 11),
                    BrandEnglishName = ReadString(reader, 12),
                    PackageName = ReadString(reader, 13),
                    UnitWeight = ReadNullableDecimal(reader, 14),
                    CartonQuantity = ReadNullableInt(reader, 15),
                    PackagingPricePerPack = ReadNullableDecimal(reader, 16),
                    KgCashPrice = ReadNullableDecimal(reader, 17),
                    KgCreditPrice = ReadNullableDecimal(reader, 18),
                    PurchaseKgCashPrice = ReadNullableDecimal(reader, 19),
                    PurchaseKgCreditPrice = ReadNullableDecimal(reader, 20),
                    ShortDescription = ReadString(reader, 21),
                    FullDescription = ReadString(reader, 22),
                    ImageUrl = ReadString(reader, 23),
                    Status = ReadString(reader, 24),
                    HaveOtherPackage = ReadNullableInt(reader, 25),
                    PackageOne = ReadString(reader, 26),
                    UpdatedAtUtc = reader.IsDBNull(27) ? null : reader.GetDateTime(27).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                });
            }
        }

        return Ok(new { page, pageSize, total, items });
    }

    [HttpPut("source-rows/{id:long}")]
    public async Task<IActionResult> UpdateSourceRow(long id, [FromQuery] string? tableName, [FromBody] SourceRowDto input, CancellationToken cancellationToken = default)
    {
        var safeTableName = NormalizeTableName(tableName);

        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await ValidateAllProductTableAsync(connection, safeTableName, cancellationToken);

        var normalizedEnglishSlug = BuildEnglishSlug(FirstNotEmpty(input.ProductEnglishName, input.ProductSlug, input.Sku, input.ProductName));
        var categorySlug = BuildEnglishSlug(FirstNotEmpty(input.CategorySlug, input.ProductEnglishName, input.BrandEnglishName, input.CategoryName));
        var mainSlug = BuildEnglishSlug(FirstNotEmpty(input.MainProductSlug, input.ProductEnglishName, categorySlug, input.MainProductName));
        var rawJson = JsonSerializer.Serialize(input, JsonOptions);
        var rowHash = ComputeSha256(rawJson + "|" + id.ToString(CultureInfo.InvariantCulture));

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
UPDATE `{safeTableName}`
SET
    SourceRowHash = @SourceRowHash,
    RawJson = @RawJson,
    MainProductName = @MainProductName,
    MainProductSlug = @MainProductSlug,
    GroupName = @GroupName,
    CategoryName = @CategoryName,
    CategorySlug = @CategorySlug,
    ProductName = @ProductName,
    ProductEnglishName = @ProductEnglishName,
    ProductSlug = @ProductSlug,
    SKU = @SKU,
    BrandName = @BrandName,
    BrandEnglishName = @BrandEnglishName,
    PackageName = @PackageName,
    UnitWeight = @UnitWeight,
    CartonQuantity = @CartonQuantity,
    PacksPerCarton = @CartonQuantity,
    PackagingPricePerPack = @PackagingPricePerPack,
    SalePriceCash = @KgCashPrice,
    SalePriceInstallment = @KgCreditPrice,
    PurchasePriceCash = @PurchaseKgCashPrice,
    PurchasePriceInstallment = @PurchaseKgCreditPrice,
    ShortDescription = @ShortDescription,
    FullDescription = @FullDescription,
    ImageUrl = @ImageUrl,
    Status = @Status,
    HaveOtherPackage = @HaveOtherPackage,
    PackageOne = @PackageOne,
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE Id = @Id;";

        Add(command, "@Id", id);
        Add(command, "@SourceRowHash", rowHash);
        Add(command, "@RawJson", rawJson);
        Add(command, "@MainProductName", input.MainProductName);
        Add(command, "@MainProductSlug", mainSlug);
        Add(command, "@GroupName", input.GroupName);
        Add(command, "@CategoryName", input.CategoryName);
        Add(command, "@CategorySlug", categorySlug);
        Add(command, "@ProductName", input.ProductName);
        Add(command, "@ProductEnglishName", input.ProductEnglishName);
        Add(command, "@ProductSlug", normalizedEnglishSlug);
        Add(command, "@SKU", input.Sku);
        Add(command, "@BrandName", input.BrandName);
        Add(command, "@BrandEnglishName", input.BrandEnglishName);
        Add(command, "@PackageName", input.PackageName);
        Add(command, "@UnitWeight", input.UnitWeight);
        Add(command, "@CartonQuantity", input.CartonQuantity);
        Add(command, "@PackagingPricePerPack", input.PackagingPricePerPack);
        Add(command, "@KgCashPrice", input.KgCashPrice);
        Add(command, "@KgCreditPrice", input.KgCreditPrice);
        Add(command, "@PurchaseKgCashPrice", input.PurchaseKgCashPrice);
        Add(command, "@PurchaseKgCreditPrice", input.PurchaseKgCreditPrice);
        Add(command, "@ShortDescription", input.ShortDescription);
        Add(command, "@FullDescription", input.FullDescription);
        Add(command, "@ImageUrl", input.ImageUrl);
        Add(command, "@Status", input.Status);
        Add(command, "@HaveOtherPackage", input.HaveOtherPackage);
        Add(command, "@PackageOne", input.PackageOne);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return Ok(new { id, affected, productSlug = normalizedEnglishSlug, categorySlug, mainSlug });
    }

    [HttpGet("source-table-summary")]
    public async Task<IActionResult> SourceTableSummary([FromQuery] string? tableName, CancellationToken cancellationToken)
    {
        var safeTableName = NormalizeTableName(tableName);
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await ValidateAllProductTableAsync(connection, safeTableName, cancellationToken);

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
        pageSize = Math.Clamp(pageSize, 5, 100);
        var offset = (page - 1) * pageSize;

        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await ValidateProductManagementTablesAsync(connection, cancellationToken);

        var where = string.IsNullOrWhiteSpace(search)
            ? ""
            : "WHERE p.ProductName LIKE @Search OR p.SKU LIKE @Search OR p.ProductSlug LIKE @Search OR g.MainProductName LIKE @Search OR p.BrandName LIKE @Search OR g.CategoryName LIKE @Search";

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
    g.MainProductName,
    p.BrandName,
    g.CategoryName
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
                    mainProductName = ReadString(reader, 20),
                    brandName = ReadString(reader, 21),
                    categoryName = ReadString(reader, 22)
                });
            }
        }

        return Ok(new { page, pageSize, total, items });
    }


    [HttpPost("woocommerce/sync-all")]
    public async Task<IActionResult> SyncWooCommerceAll(
        [FromQuery] int profileId,
        [FromQuery] bool productsOnly = false,
        [FromQuery] bool skipConnectionTest = false,
        [FromQuery] Guid? jobId = null,
        CancellationToken cancellationToken = default)
    {
        var job = await GetOrCreateJobAsync(jobId, "sync", cancellationToken);
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        try
        {
            await ValidateProductManagementTablesAsync(connection, cancellationToken);
        }
        catch (Exception ex)
        {
            await _jobs.FailAsync(job, "Validating database schema", ex, cancellationToken);
            return Conflict(new { jobId = job.JobId, error = ex.Message });
        }

        WooProfileConnection profile;
        try
        {
            profile = await _profiles.GetConnectionAsync(profileId, cancellationToken);
        }
        catch (Exception ex)
        {
            await _jobs.FailAsync(job, "Loading WooCommerce profile", ex, cancellationToken);
            return BadRequest(new { jobId = job.JobId, error = ex.Message });
        }
        using var wooClient = new ProfileWooCommerceClient(profile);
        var progressBefore = await GetWooProductSyncProgressAsync(connection, cancellationToken);
        await _jobs.StartAsync(job, "Testing WooCommerce connection", progressBefore.Total, $"Using profile '{profile.ProfileName}'.", cancellationToken);

        if (!skipConnectionTest)
        {
            var test = await wooClient.TestAsync(cancellationToken);
            await _profiles.RecordTestAsync(profileId, test, cancellationToken);
            if (!test.Success)
            {
                await _jobs.FailAsync(job, "Testing WooCommerce connection", new InvalidOperationException(test.Message), cancellationToken);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, test);
            }
        }

        var result = new WooSyncResult();

        if (!productsOnly)
        {
            await _jobs.SetPhaseAsync(job, "Sending categories to WooCommerce", 10, progressBefore.Total, 0, 0, 0, 0, 0, null, cancellationToken);
            await SyncWooCategoriesAsync(connection, wooClient, result, job, cancellationToken);
            await _jobs.SetPhaseAsync(job, "Sending base products to WooCommerce", 25, progressBefore.Total, 0, result.CategoriesSynced, result.CategoriesFailed, 0, 0, null, cancellationToken);
            await SyncWooCommoditiesAsync(connection, wooClient, result, job, cancellationToken);
            await _jobs.SetPhaseAsync(job, "Sending packaging records to WooCommerce", 40, progressBefore.Total, 0, result.CategoriesSynced + result.CommoditiesSynced, result.CategoriesFailed + result.CommoditiesFailed, 0, 0, null, cancellationToken);
            await SyncWooPackagesAsync(connection, wooClient, result, job, cancellationToken);
        }

        await _jobs.SetPhaseAsync(job, "Sending final products to WooCommerce", 55, progressBefore.Total, 0, 0, result.CategoriesFailed + result.CommoditiesFailed + result.PackagesFailed, 0, 0, null, cancellationToken);
        await SyncWooProductsAsync(connection, wooClient, result, job, cancellationToken);
        var finalProgress = await GetWooProductSyncProgressAsync(connection, cancellationToken);
        job.TotalItems = finalProgress.Total;
        job.ProcessedItems = finalProgress.Synced + finalProgress.Failed;
        job.SuccessCount = finalProgress.Synced;
        job.ErrorCount = finalProgress.Failed + result.CategoriesFailed + result.CommoditiesFailed + result.PackagesFailed;
        job.PendingCount = finalProgress.Pending;
        job.DraftCount = await GetWooDraftCountAsync(connection, cancellationToken);
        result.FinishedAtUtc = DateTime.UtcNow;
        await _jobs.CompleteAsync(job, $"WooCommerce sync completed. Synced: {finalProgress.Synced}; failed: {job.ErrorCount}; pending: {finalProgress.Pending}.", cancellationToken);
        return Ok(new { jobId = job.JobId, result, progress = finalProgress });
    }

    [HttpPost("woocommerce/sync-products")]
    public Task<IActionResult> SyncWooCommerceProductsOnly(
        [FromQuery] int profileId,
        [FromQuery] bool skipConnectionTest = false,
        [FromQuery] Guid? jobId = null,
        CancellationToken cancellationToken = default)
    {
        return SyncWooCommerceAll(profileId, productsOnly: true, skipConnectionTest, jobId, cancellationToken);
    }


    [HttpPost("woocommerce/sync-products-batch")]
    public async Task<IActionResult> SyncWooCommerceProductsBatch(
        [FromQuery] int profileId,
        [FromQuery] int take = 1,
        [FromQuery] bool skipConnectionTest = false,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 50);

        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await ValidateProductManagementTablesAsync(connection, cancellationToken);

        var profile = await _profiles.GetConnectionAsync(profileId, cancellationToken);
        using var wooClient = new ProfileWooCommerceClient(profile);
        if (!skipConnectionTest)
        {
            var test = await wooClient.TestAsync(cancellationToken);
            if (!test.Success) return StatusCode(StatusCodes.Status503ServiceUnavailable, test);
        }

        var result = new WooSyncResult();

        await SyncWooProductsAsync(connection, wooClient, result, null, cancellationToken, take, pendingOnly: true);
        var progress = await GetWooProductSyncProgressAsync(connection, cancellationToken);
        result.FinishedAtUtc = DateTime.UtcNow;

        return Ok(new
        {
            result.ProductsSynced,
            result.ProductsFailed,
            result.Errors,
            progress.Total,
            progress.Synced,
            progress.Failed,
            progress.Pending,
            progress.Percent
        });
    }

    [HttpGet("woocommerce/sync-status")]
    public async Task<IActionResult> WooCommerceSyncStatus(CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await ValidateProductManagementTablesAsync(connection, cancellationToken);

        var rows = new List<object>();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT QueueStatus, COUNT(*) AS Total
FROM khb_product_update_queue
GROUP BY QueueStatus
ORDER BY QueueStatus;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new
            {
                status = ReadString(reader, 0),
                total = reader.IsDBNull(1) ? 0 : reader.GetInt64(1)
            });
        }
        return Ok(rows);
    }


    [HttpGet("woocommerce/sync-progress")]
    public async Task<IActionResult> WooCommerceSyncProgress(CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await ValidateProductManagementTablesAsync(connection, cancellationToken);

        return Ok(await GetWooProductSyncProgressAsync(connection, cancellationToken));
    }

    [HttpGet("woocommerce/test-connection")]
    public async Task<IActionResult> TestWooCommerceConnection([FromQuery] int profileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _profiles.GetConnectionAsync(profileId, cancellationToken);
            using var wooClient = new ProfileWooCommerceClient(profile);
            var result = await wooClient.TestAsync(cancellationToken);
            await _profiles.RecordTestAsync(profileId, result, cancellationToken);
            return result.Success ? Ok(result) : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WooCommerce connection test failed before sync.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("woocommerce/sync-catalog-setup")]
    public async Task<IActionResult> SyncWooCommerceCatalogSetup([FromQuery] int profileId, [FromQuery] bool skipConnectionTest = false, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await ValidateProductManagementTablesAsync(connection, cancellationToken);

        var profile = await _profiles.GetConnectionAsync(profileId, cancellationToken);
        using var wooClient = new ProfileWooCommerceClient(profile);

        if (!skipConnectionTest)
        {
            var test = await wooClient.TestAsync(cancellationToken);
            if (!test.Success) return StatusCode(StatusCodes.Status503ServiceUnavailable, test);
        }

        var result = new WooSyncResult();
        await SyncWooCategoriesAsync(connection, wooClient, result, null, cancellationToken);
        await SyncWooCommoditiesAsync(connection, wooClient, result, null, cancellationToken);
        await SyncWooPackagesAsync(connection, wooClient, result, null, cancellationToken);
        result.FinishedAtUtc = DateTime.UtcNow;
        return Ok(result);
    }

    private async Task<KhbWorkflowJob> GetOrCreateJobAsync(
        Guid? jobId,
        string type,
        CancellationToken cancellationToken)
    {
        if (jobId.HasValue)
        {
            return await _jobs.RequireAsync(jobId.Value, type, cancellationToken);
        }

        var created = await _jobs.CreateAsync(type, User.Identity?.Name, cancellationToken);
        return await _jobs.RequireAsync(created.JobId, type, cancellationToken);
    }

    private async Task LogWooFailureAsync(
        KhbWorkflowJob job,
        string step,
        string entityType,
        string entityId,
        string? sku,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var woo = exception as WooCommerceProfileException;
        await _jobs.AddLogAsync(
            job,
            step,
            entityType,
            entityId,
            sku,
            "error",
            exception.Message,
            woo?.RequestUrl,
            woo?.ResponseCode,
            woo?.ResponseSummary,
            cancellationToken);
    }

    private static async Task<int> GetWooDraftCountAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM khb_product_final WHERE COALESCE(Status, 'draft') <> 'publish';";
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    private async Task SyncWooCategoriesAsync(
        MySqlConnection connection,
        ProfileWooCommerceClient wooClient,
        WooSyncResult result,
        KhbWorkflowJob? job,
        CancellationToken cancellationToken)
    {
        var rows = new List<WooCategoryRow>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
SELECT Id, SourceKey, CategoryName, CategorySlug, WooCategoryId
FROM khb_category_map
ORDER BY Id;";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(new WooCategoryRow(
                    reader.GetInt64(0),
                    ReadString(reader, 1) ?? string.Empty,
                    ReadString(reader, 2),
                    ReadString(reader, 3),
                    ReadNullableLong(reader, 4)));
            }
        }

        foreach (var row in rows)
        {
            try
            {
                var name = FirstNotEmpty(row.CategoryName, row.CategorySlug, "Kharbarchi");
                var slug = BuildEnglishSlug(FirstNotEmpty(row.CategorySlug, row.CategoryName, row.SourceKey));
                var payload = new JsonObject
                {
                    ["name"] = name,
                    ["slug"] = slug,
                    ["english_name"] = slug
                };

                // Use the plugin endpoint so product_cat and its import meta are normalized in one place.
                var wooId = await PostKharbarchiEndpointAsync(wooClient, "/wp-json/khb/v1/category/upsert", row.WooCategoryId, payload, cancellationToken);

                await ExecuteUpsertAsync(connection, "UPDATE khb_category_map SET WooCategoryId = @WooCategoryId, UpdatedAtUtc = UTC_TIMESTAMP(6) WHERE Id = @Id;", cancellationToken,
                    ("@WooCategoryId", wooId),
                    ("@Id", row.Id));
                result.CategoriesSynced++;
                if (job is not null)
                {
                    await _jobs.AddLogAsync(job, "Sending categories to WooCommerce", "category", row.SourceKey, null, "success", $"WooCommerce category id {wooId}.", wooClient.BuildSafeUrl("/wp-json/khb/v1/category/upsert"), 200, null, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                result.CategoriesFailed++;
                result.Errors.Add($"category:{row.SourceKey}: {ex.Message}");
                if (job is not null)
                {
                    await LogWooFailureAsync(job, "Sending categories to WooCommerce", "category", row.SourceKey, null, ex, cancellationToken);
                }
            }
        }
    }

    private async Task SyncWooCommoditiesAsync(
        MySqlConnection connection,
        ProfileWooCommerceClient wooClient,
        WooSyncResult result,
        KhbWorkflowJob? job,
        CancellationToken cancellationToken)
    {
        var rows = new List<WooCommodityRow>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
SELECT
    c.Id,
    c.SourceKey,
    c.CommodityName,
    c.CommoditySlug,
    c.WooCommodityId,
    MAX(cat.WooCategoryId) AS WooCategoryId,
    MAX(cat.CategorySlug) AS CategorySlug,
    MAX(cat.CategoryName) AS CategoryName
FROM khb_commodity c
LEFT JOIN khb_product_final p ON p.CommoditySourceKey = c.SourceKey
LEFT JOIN khb_category_map cat ON cat.SourceKey = p.CategorySourceKey
GROUP BY c.Id, c.SourceKey, c.CommodityName, c.CommoditySlug, c.WooCommodityId
ORDER BY c.Id;";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(new WooCommodityRow(
                    reader.GetInt64(0),
                    ReadString(reader, 1) ?? string.Empty,
                    ReadString(reader, 2),
                    ReadString(reader, 3),
                    ReadNullableLong(reader, 4),
                    ReadNullableLong(reader, 5),
                    ReadString(reader, 6),
                    ReadString(reader, 7)));
            }
        }

        foreach (var row in rows)
        {
            try
            {
                var commoditySlug = BuildEnglishSlug(FirstNotEmpty(row.CommoditySlug, row.CommodityName, row.SourceKey));
                var payload = new JsonObject
                {
                    ["source_key"] = row.SourceKey,
                    ["name"] = FirstNotEmpty(row.CommodityName, row.CommoditySlug, row.SourceKey),
                    ["slug"] = commoditySlug,
                    ["english_name"] = commoditySlug,
                    ["category_slug"] = BuildEnglishSlug(FirstNotEmpty(row.CategorySlug, row.CategoryName))
                };

                var wooId = await PostKharbarchiEndpointAsync(wooClient, "/wp-json/khb/v1/commodity/upsert", row.WooCommodityId, payload, cancellationToken);
                await ExecuteUpsertAsync(connection, "UPDATE khb_commodity SET WooCommodityId = @WooCommodityId, UpdatedAtUtc = UTC_TIMESTAMP(6) WHERE Id = @Id;", cancellationToken,
                    ("@WooCommodityId", wooId),
                    ("@Id", row.Id));
                result.CommoditiesSynced++;
                if (job is not null)
                {
                    await _jobs.AddLogAsync(job, "Sending base products to WooCommerce", "commodity", row.SourceKey, null, "success", $"WooCommerce commodity id {wooId}.", wooClient.BuildSafeUrl("/wp-json/khb/v1/commodity/upsert"), 200, null, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                result.CommoditiesFailed++;
                result.Errors.Add($"commodity:{row.SourceKey}: {ex.Message}");
                if (job is not null)
                {
                    await LogWooFailureAsync(job, "Sending base products to WooCommerce", "commodity", row.SourceKey, null, ex, cancellationToken);
                }
            }
        }
    }

    private async Task SyncWooPackagesAsync(
        MySqlConnection connection,
        ProfileWooCommerceClient wooClient,
        WooSyncResult result,
        KhbWorkflowJob? job,
        CancellationToken cancellationToken)
    {
        var rows = new List<WooPackageRow>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
SELECT Id, SourceKey, PackageGroup, PackageCode, PackageTitle, UnitWeightKg, PacksPerCarton, PackagingPricePerPack, WooPackageId
FROM khb_package_type
ORDER BY Id;";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(new WooPackageRow(
                    reader.GetInt64(0),
                    ReadString(reader, 1) ?? string.Empty,
                    ReadString(reader, 2),
                    ReadString(reader, 3),
                    ReadString(reader, 4),
                    ReadNullableDecimal(reader, 5),
                    ReadNullableInt(reader, 6),
                    ReadNullableDecimal(reader, 7),
                    ReadNullableLong(reader, 8)));
            }
        }

        foreach (var row in rows)
        {
            try
            {
                var group = NormalizePackageGroup(row.PackageGroup, row.PackageCode);
                var code = FirstNotEmpty(row.PackageCode, row.SourceKey);
                var title = FirstNotEmpty(row.PackageTitle, row.PackageCode, row.SourceKey);
                var payload = new JsonObject
                {
                    ["source_key"] = row.SourceKey,
                    ["title"] = title,
                    ["package_code"] = code,
                    ["package_group"] = group,
                    ["unit_weight"] = ToMetaDecimal(row.UnitWeightKg),
                    ["default_carton_count"] = row.PacksPerCarton ?? 0,
                    ["image_tag"] = BuildImageTag(row.PackageTitle, group, row.UnitWeightKg)
                };

                var wooId = await PostKharbarchiEndpointAsync(wooClient, "/wp-json/khb/v1/package/upsert", row.WooPackageId, payload, cancellationToken);
                await ExecuteUpsertAsync(connection, "UPDATE khb_package_type SET WooPackageId = @WooPackageId, UpdatedAtUtc = UTC_TIMESTAMP(6) WHERE Id = @Id;", cancellationToken,
                    ("@WooPackageId", wooId > 0 ? wooId : null),
                    ("@Id", row.Id));
                result.PackagesSynced++;
                if (job is not null)
                {
                    await _jobs.AddLogAsync(job, "Sending packaging records to WooCommerce", "package", row.SourceKey, null, "success", wooId > 0 ? $"WooCommerce package id {wooId}." : "Package endpoint completed.", wooClient.BuildSafeUrl("/wp-json/khb/v1/package/upsert"), 200, null, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                result.PackagesFailed++;
                result.Errors.Add($"package:{row.SourceKey}: {ex.Message}");
                if (job is not null)
                {
                    await LogWooFailureAsync(job, "Sending packaging records to WooCommerce", "package", row.SourceKey, null, ex, cancellationToken);
                }
            }
        }
    }

    private async Task SyncWooProductsAsync(
        MySqlConnection connection,
        ProfileWooCommerceClient wooClient,
        WooSyncResult result,
        KhbWorkflowJob? job,
        CancellationToken cancellationToken,
        int? take = null,
        bool pendingOnly = false)
    {
        var rows = new List<WooProductRow>();
        await using (var command = connection.CreateCommand())
        {
            var where = pendingOnly
                ? "\nWHERE COALESCE(q.QueueStatus, 'pending') IN ('pending', 'failed')"
                : string.Empty;
            var limit = take.HasValue ? "\nLIMIT @Take" : string.Empty;

            command.CommandText = $@"
SELECT
    p.Id,
    p.SourceKey,
    p.WooProductId,
    p.SKU,
    p.ProductSlug,
    p.WooPayloadJson,
    p.CategorySourceKey,
    p.CommoditySourceKey,
    p.PackageSourceKey,
    c.WooCategoryId,
    c.CategoryName,
    c.CategorySlug,
    cm.WooCommodityId,
    cm.CommodityName,
    cm.CommoditySlug,
    q.WooProductId AS QueueWooProductId,
    p.ProductName,
    p.ProductEnglishName,
    p.PackageGroup,
    p.PackageCode,
    COALESCE(pk.PackageTitle, sp.PackageName, p.PackageCode) AS PackageTitle,
    pk.WooPackageId,
    p.UnitWeightKg,
    p.PacksPerCarton,
    p.PackagingPricePerPack,
    p.KgCashPrice,
    p.KgCreditPrice,
    p.SaleCashPrice,
    p.SaleCreditPrice,
    p.BuyCashPrice,
    p.BuyCreditPrice,
    p.Status,
    p.CatalogVisibility,
    sp.BrandName,
    sp.BrandEnglishName,
    sp.ShortDescription,
    sp.FullDescription,
    sp.ImageUrl,
    sp.GalleryJson
FROM khb_product_final p
LEFT JOIN khb_category_map c ON c.SourceKey = p.CategorySourceKey
LEFT JOIN khb_commodity cm ON cm.SourceKey = p.CommoditySourceKey
LEFT JOIN khb_package_type pk ON pk.SourceKey = p.PackageSourceKey
LEFT JOIN khb_product_update_queue q ON q.SourceKey = p.SourceKey
LEFT JOIN khb_sale_products sp ON sp.SourceRowHash = p.SourceKey
{where}
ORDER BY p.Id{limit};";

            if (take.HasValue)
            {
                Add(command, "@Take", Math.Clamp(take.Value, 1, 50));
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(new WooProductRow(
                    reader.GetInt64(0),
                    ReadString(reader, 1) ?? string.Empty,
                    ReadNullableLong(reader, 2),
                    ReadString(reader, 3),
                    ReadString(reader, 4),
                    ReadString(reader, 5),
                    ReadString(reader, 6),
                    ReadString(reader, 7),
                    ReadString(reader, 8),
                    ReadNullableLong(reader, 9),
                    ReadString(reader, 10),
                    ReadString(reader, 11),
                    ReadNullableLong(reader, 12),
                    ReadString(reader, 13),
                    ReadString(reader, 14),
                    ReadNullableLong(reader, 15),
                    ReadString(reader, 16),
                    ReadString(reader, 17),
                    ReadString(reader, 18),
                    ReadString(reader, 19),
                    ReadString(reader, 20),
                    ReadNullableLong(reader, 21),
                    ReadNullableDecimal(reader, 22),
                    ReadNullableInt(reader, 23),
                    ReadNullableDecimal(reader, 24),
                    ReadNullableDecimal(reader, 25),
                    ReadNullableDecimal(reader, 26),
                    ReadNullableDecimal(reader, 27),
                    ReadNullableDecimal(reader, 28),
                    ReadNullableDecimal(reader, 29),
                    ReadNullableDecimal(reader, 30),
                    ReadString(reader, 31),
                    ReadString(reader, 32),
                    ReadString(reader, 33),
                    ReadString(reader, 34),
                    ReadString(reader, 35),
                    ReadString(reader, 36),
                    ReadString(reader, 37),
                    ReadString(reader, 38)));
            }
        }

        foreach (var row in rows)
        {
            try
            {
                if (row.WooCategoryId is not > 0)
                {
                    throw new InvalidOperationException("DEPENDENCY_CATEGORY_NOT_SYNCED");
                }
                if (row.WooCommodityId is not > 0)
                {
                    throw new InvalidOperationException("DEPENDENCY_COMMODITY_NOT_SYNCED");
                }
                if (row.WooPackageId is not > 0)
                {
                    throw new InvalidOperationException("DEPENDENCY_PACKAGE_NOT_SYNCED");
                }

                var payload = BuildWooProductPayload(row);
                var wooId = await UpsertWooProductAsync(wooClient, row, payload, cancellationToken);

                await LinkWooProductRelationsAsync(wooClient, row, wooId, cancellationToken);

                await ExecuteUpsertAsync(connection, @"
UPDATE khb_product_final
SET WooProductId = @WooProductId, UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE Id = @Id;", cancellationToken,
                    ("@WooProductId", wooId),
                    ("@Id", row.Id));

                await ExecuteUpsertAsync(connection, @"
UPDATE khb_product_update_queue
SET EntityType = 'product', QueueStatus = 'synced', WooProductId = @WooProductId, LastError = NULL, TryCount = TryCount + 1, UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE SourceKey = @SourceKey;", cancellationToken,
                    ("@WooProductId", wooId),
                    ("@SourceKey", row.SourceKey));

                result.ProductsSynced++;
                if (job is not null)
                {
                    await _jobs.AddLogAsync(job, "Sending final products to WooCommerce", "product", row.SourceKey, row.Sku, "success", $"WooCommerce product id {wooId}.", wooClient.BuildSafeUrl("/wp-json/wc/v3/products"), 200, null, cancellationToken);
                    var processed = result.ProductsSynced + result.ProductsFailed;
                    var total = Math.Max(job.TotalItems, processed);
                    await _jobs.SetPhaseAsync(job, "Sending final products to WooCommerce", 55 + (int)Math.Round(processed * 44d / Math.Max(1, total)), total, processed, result.ProductsSynced, result.ProductsFailed, job.DraftCount, job.SkippedCount, $"Processed {processed} of {total} products.", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await ExecuteUpsertAsync(connection, @"
UPDATE khb_product_update_queue
SET EntityType = 'product', QueueStatus = 'failed', LastError = @LastError, TryCount = TryCount + 1, UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE SourceKey = @SourceKey;", cancellationToken,
                    ("@LastError", ex.Message),
                    ("@SourceKey", row.SourceKey));

                result.ProductsFailed++;
                result.Errors.Add($"product:{row.SourceKey}: {ex.Message}");
                if (job is not null)
                {
                    await LogWooFailureAsync(job, "Sending final products to WooCommerce", "product", row.SourceKey, row.Sku, ex, cancellationToken);
                }
                if (ex is WooCommerceProfileException { ResponseCode: 401 or 403 })
                {
                    if (job is not null)
                    {
                        await _jobs.FailAsync(job, "Sending final products to WooCommerce", ex, cancellationToken);
                    }
                    throw;
                }
            }
        }
    }

    private static JsonObject BuildWooProductPayload(WooProductRow row)
    {
        var node = string.IsNullOrWhiteSpace(row.WooPayloadJson)
            ? new JsonObject()
            : (JsonNode.Parse(row.WooPayloadJson)?.AsObject() ?? new JsonObject());

        var priceContext = GetWooPriceContext(row);
        var productStatus = priceContext.ShouldDraft ? "draft" : NormalizeWooStatus(row.Status);
        var catalogVisibility = productStatus == "publish" ? FirstNotEmpty(row.CatalogVisibility, "visible") : "hidden";

        node["name"] = FirstNotEmpty(row.ProductName, row.ProductEnglishName, row.Sku, row.SourceKey);
        node["slug"] = BuildEnglishSlug(FirstNotEmpty(row.ProductSlug, row.Sku, row.SourceKey));
        node["sku"] = row.Sku ?? string.Empty;
        node["type"] = "simple";
        node["status"] = productStatus;
        node["catalog_visibility"] = catalogVisibility;
        node["regular_price"] = ToWooPrice(row.SaleCreditPrice);
        node["sale_price"] = string.Empty;
        node["manage_stock"] = false;

        if (!string.IsNullOrWhiteSpace(row.ShortDescription))
        {
            node["short_description"] = row.ShortDescription;
        }
        if (!string.IsNullOrWhiteSpace(row.FullDescription))
        {
            node["description"] = row.FullDescription;
        }

        if (row.WooCategoryId.HasValue && row.WooCategoryId.Value > 0)
        {
            var categories = new JsonArray();
            categories.Add(new JsonObject { ["id"] = row.WooCategoryId.Value });
            node["categories"] = categories;
        }

        var images = BuildWooImages(row.ImageUrl, row.GalleryJson);
        if (images.Count > 0)
        {
            node["images"] = images;
        }

        node["attributes"] = BuildWooAttributes(row, priceContext);

        var meta = node["meta_data"] as JsonArray ?? new JsonArray();
        UpsertProductMetaArray(meta, row, priceContext);
        node["meta_data"] = meta;

        return node;
    }

    private async Task LinkWooProductRelationsAsync(ProfileWooCommerceClient wooClient, WooProductRow row, long wooProductId, CancellationToken cancellationToken)
    {
        var priceContext = GetWooPriceContext(row);
        var payload = new JsonObject
        {
            ["product_id"] = wooProductId,
            ["category_slug"] = BuildEnglishSlug(FirstNotEmpty(row.CategorySlug, row.CategoryName)),
            ["commodity_slug"] = BuildEnglishSlug(FirstNotEmpty(row.CommoditySlug, row.CommodityName)),
            ["package_code"] = row.PackageCode ?? string.Empty,
            ["meta"] = BuildProductLinkMeta(row, priceContext)
        };

        await SendWooJsonAsync(wooClient, HttpMethod.Post, "/wp-json/khb/v1/product/link", payload, cancellationToken);
    }

    private async Task<long> UpsertWooProductAsync(ProfileWooCommerceClient wooClient, WooProductRow row, JsonObject payload, CancellationToken cancellationToken)
    {
        var existingId = row.WooProductId ?? row.QueueWooProductId;
        if (!existingId.HasValue && !string.IsNullOrWhiteSpace(row.Sku))
        {
            existingId = await FindWooObjectIdByQueryAsync(wooClient, "/wp-json/wc/v3/products", $"sku={Uri.EscapeDataString(row.Sku)}", cancellationToken);
        }

        if (existingId.HasValue && existingId.Value > 0)
        {
            var doc = await SendWooJsonAsync(wooClient, HttpMethod.Put, $"/wp-json/wc/v3/products/{existingId.Value}", payload, cancellationToken);
            return ExtractWooId(doc) ?? existingId.Value;
        }

        var created = await SendWooJsonAsync(wooClient, HttpMethod.Post, "/wp-json/wc/v3/products", payload, cancellationToken);
        return ExtractWooId(created) ?? throw new InvalidOperationException("WooCommerce product response did not include id.");
    }

    private async Task<long> UpsertWooCommerceObjectAsync(ProfileWooCommerceClient wooClient, string endpoint, long? existingId, string? slug, JsonObject payload, CancellationToken cancellationToken)
    {
        if (existingId.HasValue && existingId.Value > 0)
        {
            var updated = await SendWooJsonAsync(wooClient, HttpMethod.Put, $"{endpoint}/{existingId.Value}", payload, cancellationToken);
            return ExtractWooId(updated) ?? existingId.Value;
        }

        if (!string.IsNullOrWhiteSpace(slug))
        {
            var found = await FindWooObjectIdByQueryAsync(wooClient, endpoint, $"slug={Uri.EscapeDataString(slug)}", cancellationToken);
            if (found.HasValue && found.Value > 0)
            {
                var updated = await SendWooJsonAsync(wooClient, HttpMethod.Put, $"{endpoint}/{found.Value}", payload, cancellationToken);
                return ExtractWooId(updated) ?? found.Value;
            }
        }

        var created = await SendWooJsonAsync(wooClient, HttpMethod.Post, endpoint, payload, cancellationToken);
        return ExtractWooId(created) ?? throw new InvalidOperationException("WooCommerce response did not include id.");
    }

    private async Task<long> PostKharbarchiEndpointAsync(ProfileWooCommerceClient wooClient, string endpoint, long? existingId, JsonObject payload, CancellationToken cancellationToken)
    {
        if (existingId.HasValue) payload["id"] = existingId.Value;
        var doc = await SendWooJsonAsync(wooClient, HttpMethod.Post, endpoint, payload, cancellationToken);
        return ExtractWooId(doc) ?? existingId ?? 0;
    }

    private static async Task<long?> FindWooObjectIdByQueryAsync(ProfileWooCommerceClient wooClient, string endpoint, string query, CancellationToken cancellationToken)
    {
        using var doc = await wooClient.TryGetJsonAsync(endpoint + (endpoint.Contains('?') ? "&" : "?") + query, cancellationToken);
        if (doc is null) return null;
        if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0) return null;
        return doc.RootElement[0].TryGetProperty("id", out var idElement) && idElement.TryGetInt64(out var id) ? id : null;
    }

    private static Task<JsonDocument> SendWooJsonAsync(
        ProfileWooCommerceClient wooClient,
        HttpMethod method,
        string endpoint,
        JsonObject payload,
        CancellationToken cancellationToken) =>
        wooClient.SendJsonAsync(method, endpoint, payload.ToJsonString(JsonOptions), cancellationToken);

    private static long? ExtractWooId(JsonDocument doc)
    {
        if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("id", out var idElement) && idElement.TryGetInt64(out var id))
        {
            return id;
        }
        if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("term_id", out var termIdElement) && termIdElement.TryGetInt64(out var termId))
        {
            return termId;
        }
        return null;
    }

    private static void AddMeta(JsonArray meta, string key, string? value)
    {
        SetMeta(meta, key, value);
    }

    private static void SetMeta(JsonArray meta, string key, object? value, bool keepEmpty = false)
    {
        var text = ConvertMetaValue(value);
        if (string.IsNullOrWhiteSpace(text) && !keepEmpty)
        {
            RemoveMeta(meta, key);
            return;
        }

        RemoveMeta(meta, key);
        meta.Add(new JsonObject
        {
            ["key"] = key,
            ["value"] = text ?? string.Empty
        });
    }

    private static void RemoveMeta(JsonArray meta, string key)
    {
        for (var i = meta.Count - 1; i >= 0; i--)
        {
            if (meta[i] is not JsonObject item || !item.TryGetPropertyValue("key", out var keyNode))
            {
                continue;
            }

            var existing = keyNode?.GetValue<string>();
            if (string.Equals(existing, key, StringComparison.Ordinal))
            {
                meta.RemoveAt(i);
            }
        }
    }

    private static string? ConvertMetaValue(object? value)
    {
        return value switch
        {
            null => null,
            decimal d => ToMetaDecimal(d),
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            bool b => b ? "1" : "0",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };
    }

    private static void SetObjectMeta(JsonObject meta, string key, object? value, bool keepEmpty = false)
    {
        var text = ConvertMetaValue(value);
        if (string.IsNullOrWhiteSpace(text) && !keepEmpty)
        {
            meta.Remove(key);
            return;
        }
        meta[key] = text ?? string.Empty;
    }

    private static JsonObject BuildProductLinkMeta(WooProductRow row, WooPriceContext context)
    {
        var meta = new JsonObject();
        FillProductMetaObject(meta, row, context);
        return meta;
    }

    private static void UpsertProductMetaArray(JsonArray meta, WooProductRow row, WooPriceContext context)
    {
        var obj = new JsonObject();
        FillProductMetaObject(obj, row, context);
        foreach (var item in obj)
        {
            SetMeta(meta, item.Key, item.Value?.GetValue<string>(), keepEmpty: true);
        }
    }

    private static void FillProductMetaObject(JsonObject meta, WooProductRow row, WooPriceContext context)
    {
        SetObjectMeta(meta, "_kharbarchi_sale_credit_price", row.SaleCreditPrice);
        SetObjectMeta(meta, "_kharbarchi_sale_cash_price", row.SaleCashPrice);
        SetObjectMeta(meta, "_kharbarchi_buy_credit_price", row.BuyCreditPrice);
        SetObjectMeta(meta, "_kharbarchi_buy_cash_price", row.BuyCashPrice);

        SetObjectMeta(meta, "_kharbarchi_sale_credit_price_per_kg", row.KgCreditPrice);
        SetObjectMeta(meta, "_kharbarchi_sale_cash_price_per_kg", row.KgCashPrice);
        SetObjectMeta(meta, "_kharbarchi_buy_credit_price_per_kg", SafePerKg(row.BuyCreditPrice, context.TotalWeight));
        SetObjectMeta(meta, "_kharbarchi_buy_cash_price_per_kg", SafePerKg(row.BuyCashPrice, context.TotalWeight));

        SetObjectMeta(meta, "_khb_package_code", row.PackageCode);
        SetObjectMeta(meta, "_khb_package_title", FirstNotEmpty(row.PackageTitle, row.PackageCode));
        SetObjectMeta(meta, "_khb_package_group", context.PackageGroup);
        SetObjectMeta(meta, "_khb_sale_mode", GetSaleMode(context.PackageGroup));
        SetObjectMeta(meta, "_khb_price_calculation_basis", GetPriceCalculationBasis(context.PackageGroup));
        SetObjectMeta(meta, "_khb_unit_weight", row.UnitWeightKg);
        SetObjectMeta(meta, "_khb_product_carton_count", context.PacksPerCarton);
        SetObjectMeta(meta, "_khb_bulk_weight_kg", context.BulkWeightKg);
        SetObjectMeta(meta, "_khb_min_purchase_kg", context.MinPurchaseKg);
        SetObjectMeta(meta, "_khb_image_tag", BuildImageTag(row.PackageTitle, context.PackageGroup, row.UnitWeightKg));

        SetObjectMeta(meta, "_kharbarchi_min_cartons", 1);
        SetObjectMeta(meta, "_kharbarchi_max_cartons", 0);
        SetObjectMeta(meta, "_kharbarchi_carton_step", 1);

        SetObjectMeta(meta, "_kharbarchi_brand_name", row.BrandName);
        SetObjectMeta(meta, "_kharbarchi_brand_english_name", row.BrandEnglishName);
        SetObjectMeta(meta, "_kharbarchi_commodity_name", row.CommodityName);
        SetObjectMeta(meta, "_kharbarchi_commodity_slug", row.CommoditySlug);
        SetObjectMeta(meta, "_kharbarchi_category_source_key", row.CategorySourceKey);
        SetObjectMeta(meta, "_kharbarchi_commodity_source_key", row.CommoditySourceKey);
        SetObjectMeta(meta, "_kharbarchi_package_source_key", row.PackageSourceKey);
        SetObjectMeta(meta, "_kharbarchi_woo_commodity_id", row.WooCommodityId);

        SetObjectMeta(meta, "_khb_price_source_mode", "final_price");
        SetObjectMeta(meta, "_khb_source_id", row.SourceKey);
        SetObjectMeta(meta, "_khb_source_row_number", row.Id);
        SetObjectMeta(meta, "_khb_need_fix", context.ShouldDraft ? 1 : 0);
        SetObjectMeta(meta, "_khb_fix_note", context.FixNote);
        SetObjectMeta(meta, "woodmart_price_unit_of_measure", context.UnitMeasure, keepEmpty: true);
    }

    private static JsonArray BuildWooAttributes(WooProductRow row, WooPriceContext context)
    {
        var attributes = new JsonArray();
        AddVisibleAttribute(attributes, "برند", row.BrandName);
        AddVisibleAttribute(attributes, "کالای پایه", row.CommodityName);
        AddVisibleAttribute(attributes, "بسته‌بندی", FirstNotEmpty(row.PackageTitle, row.PackageCode));
        AddVisibleAttribute(attributes, "واحد فروش", context.UnitMeasure);
        AddVisibleAttribute(attributes, context.PackageGroup == "bulk" ? "وزن کارتن" : "تعداد بسته در کارتن", context.QuantityLabel);
        if (context.PackageGroup == "bulk")
        {
            AddVisibleAttribute(attributes, "قیمت کیلویی فروش", FormatToman(row.KgCreditPrice));
            AddVisibleAttribute(attributes, "تخفیف نقدی کیلویی", FormatToman(GetCashDiscount(row.KgCreditPrice, row.KgCashPrice)));
        }
        else
        {
            AddVisibleAttribute(attributes, "قیمت بسته فروش", FormatToman(row.KgCreditPrice));
            AddVisibleAttribute(attributes, "تخفیف نقدی هر بسته", FormatToman(GetCashDiscount(row.KgCreditPrice, row.KgCashPrice)));
        }
        return attributes;
    }

    private static void AddVisibleAttribute(JsonArray attributes, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var options = new JsonArray();
        options.Add(value.Trim());
        attributes.Add(new JsonObject
        {
            ["name"] = name,
            ["visible"] = true,
            ["variation"] = false,
            ["options"] = options
        });
    }

    private static JsonArray BuildWooImages(string? imageUrl, string? galleryJson)
    {
        var images = new JsonArray();
        AddImageIfUrl(images, imageUrl);

        if (!string.IsNullOrWhiteSpace(galleryJson))
        {
            try
            {
                var node = JsonNode.Parse(galleryJson);
                if (node is JsonArray arr)
                {
                    foreach (var item in arr)
                    {
                        AddImageIfUrl(images, item?.ToString());
                    }
                }
            }
            catch
            {
                foreach (var part in galleryJson.Split(new[] { ',', ';', '|', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    AddImageIfUrl(images, part.Trim());
                }
            }
        }

        return images;
    }

    private static void AddImageIfUrl(JsonArray images, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        url = url.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return;
        }

        images.Add(new JsonObject { ["src"] = url });
    }

    private static WooPriceContext GetWooPriceContext(WooProductRow row)
    {
        var group = NormalizePackageGroup(row.PackageGroup, row.PackageCode);
        var unitWeight = row.UnitWeightKg.GetValueOrDefault();
        var packs = row.PacksPerCarton.GetValueOrDefault();
        var bulkWeight = 0m;
        var minPurchase = 0m;
        var totalWeight = 0m;
        var quantityLabel = string.Empty;
        var unitMeasure = group == "bulk" ? "کارتن وزنی" : "کارتن";
        var shouldDraft = false;
        var notes = new List<string>();

        if (!row.SaleCreditPrice.HasValue || row.SaleCreditPrice.Value <= 0)
        {
            shouldDraft = true;
            notes.Add("MISSING_SALE_CREDIT_PRICE");
        }

        if (row.SaleCashPrice.HasValue && row.SaleCreditPrice.HasValue && row.SaleCashPrice.Value > row.SaleCreditPrice.Value)
        {
            shouldDraft = true;
            notes.Add("CASH_GREATER_THAN_CREDIT");
        }

        if (string.IsNullOrWhiteSpace(row.CommoditySlug))
        {
            shouldDraft = true;
            notes.Add("MISSING_COMMODITY");
        }

        if (string.IsNullOrWhiteSpace(row.PackageCode))
        {
            shouldDraft = true;
            notes.Add("MISSING_PACKAGE");
        }

        if (group == "retail")
        {
            if (unitWeight <= 0)
            {
                shouldDraft = true;
                notes.Add("MISSING_UNIT_WEIGHT");
            }
            if (packs <= 0)
            {
                shouldDraft = true;
                notes.Add("MISSING_CARTON_COUNT");
            }
            if (unitWeight > 0 && packs > 0)
            {
                totalWeight = unitWeight * packs;
                quantityLabel = packs.ToString(CultureInfo.InvariantCulture) + " بسته";
            }
        }
        else if (group == "bulk")
        {
            bulkWeight = unitWeight;
            minPurchase = unitWeight;
            if (bulkWeight <= 0)
            {
                shouldDraft = true;
                notes.Add("MISSING_BULK_WEIGHT");
            }
            totalWeight = bulkWeight;
            quantityLabel = totalWeight > 0 ? ToMetaDecimal(totalWeight) + " کیلو" : string.Empty;
        }
        else
        {
            shouldDraft = true;
            notes.Add("INVALID_PACKAGE_CODE");
            if (unitWeight > 0)
            {
                totalWeight = unitWeight;
                quantityLabel = ToMetaDecimal(unitWeight) + " کیلو";
            }
        }

        return new WooPriceContext(
            PackageGroup: group,
            TotalWeight: totalWeight,
            BulkWeightKg: bulkWeight,
            MinPurchaseKg: minPurchase,
            PacksPerCarton: packs,
            UnitMeasure: unitMeasure,
            QuantityLabel: quantityLabel,
            ShouldDraft: shouldDraft,
            FixNote: string.Join(",", notes.Distinct(StringComparer.Ordinal)));
    }

    private static string GetSaleMode(string? packageGroup)
    {
        var group = (packageGroup ?? string.Empty).Trim().ToLowerInvariant();
        return group switch
        {
            "bulk" => "carton_weight",
            "retail" => "carton_packaged",
            _ => "no_package_pending"
        };
    }

    private static string GetPriceCalculationBasis(string? packageGroup)
    {
        var group = (packageGroup ?? string.Empty).Trim().ToLowerInvariant();
        return group switch
        {
            "bulk" => "kg",
            "retail" => "package",
            _ => "manual_review"
        };
    }

    private static string NormalizePackageGroup(string? group, string? code)
    {
        group = (group ?? string.Empty).Trim().ToLowerInvariant();
        code = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (group is "bulk" or "retail" or "none") return group;
        if (code is "450" or "900" or "450G" or "900G") return "retail";
        if (code.StartsWith("B", StringComparison.OrdinalIgnoreCase)) return "bulk";
        if (code == "NOPKG") return "none";
        return string.IsNullOrWhiteSpace(group) ? "none" : group;
    }

    private static string NormalizeWooStatus(string? status)
    {
        status = (status ?? string.Empty).Trim().ToLowerInvariant();
        return status is "publish" or "private" or "draft" or "pending" ? status : "publish";
    }

    private static string? ToWooPrice(decimal? value)
    {
        return value.HasValue && value.Value > 0 ? value.Value.ToString("0", CultureInfo.InvariantCulture) : string.Empty;
    }

    private static string ToMetaDecimal(decimal? value)
    {
        return value.HasValue ? ToMetaDecimal(value.Value) : string.Empty;
    }

    private static string ToMetaDecimal(decimal value)
    {
        var text = value.ToString("0.####", CultureInfo.InvariantCulture);
        return text == "-0" ? "0" : text;
    }

    private static decimal? SafePerKg(decimal? finalPrice, decimal totalWeight)
    {
        if (!finalPrice.HasValue || finalPrice.Value <= 0 || totalWeight <= 0) return null;
        return Math.Round(finalPrice.Value / totalWeight, 2);
    }

    private static decimal? GetCashDiscount(decimal? credit, decimal? cash)
    {
        if (!credit.HasValue || !cash.HasValue) return null;
        var diff = credit.Value - cash.Value;
        return diff > 0 ? diff : 0;
    }

    private static string? FormatToman(decimal? value)
    {
        return value.HasValue && value.Value > 0
            ? value.Value.ToString("N0", CultureInfo.InvariantCulture) + " تومان"
            : null;
    }

    private static string BuildImageTag(string? packageTitle, string group, decimal? unitWeight)
    {
        if (!string.IsNullOrWhiteSpace(packageTitle)) return packageTitle.Trim();
        if (group == "retail" && unitWeight.HasValue) return (unitWeight.Value * 1000m).ToString("0", CultureInfo.InvariantCulture) + " گرمی";
        if (group == "bulk") return "فله";
        return string.Empty;
    }

    private static async Task<WooProductSyncProgress> GetWooProductSyncProgressAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    COUNT(*) AS Total,
    SUM(CASE WHEN QueueStatus = 'synced' THEN 1 ELSE 0 END) AS Synced,
    SUM(CASE WHEN QueueStatus = 'failed' THEN 1 ELSE 0 END) AS Failed,
    SUM(CASE WHEN QueueStatus IN ('pending', 'processing') OR QueueStatus IS NULL THEN 1 ELSE 0 END) AS Pending
FROM khb_product_update_queue
WHERE EntityType = 'product'
   OR ActionType = 'upsert';";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new WooProductSyncProgress(0, 0, 0, 0, 0);
        }

        var total = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture);
        var synced = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture);
        var failed = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2), CultureInfo.InvariantCulture);
        var pending = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3), CultureInfo.InvariantCulture);
        var percent = total <= 0 ? 0 : (int)Math.Round((synced + failed) * 100.0 / total);
        return new WooProductSyncProgress(total, synced, failed, pending, percent);
    }

    private sealed record WooPriceContext(string PackageGroup, decimal TotalWeight, decimal BulkWeightKg, decimal MinPurchaseKg, int PacksPerCarton, string UnitMeasure, string QuantityLabel, bool ShouldDraft, string FixNote);
    private sealed record WooProductSyncProgress(int Total, int Synced, int Failed, int Pending, int Percent);
    private sealed record WooCategoryRow(long Id, string SourceKey, string? CategoryName, string? CategorySlug, long? WooCategoryId);
    private sealed record WooCommodityRow(long Id, string SourceKey, string? CommodityName, string? CommoditySlug, long? WooCommodityId, long? WooCategoryId, string? CategorySlug, string? CategoryName);
    private sealed record WooPackageRow(long Id, string SourceKey, string? PackageGroup, string? PackageCode, string? PackageTitle, decimal? UnitWeightKg, int? PacksPerCarton, decimal? PackagingPricePerPack, long? WooPackageId);
    private sealed record WooProductRow(long Id, string SourceKey, long? WooProductId, string? Sku, string? ProductSlug, string? WooPayloadJson, string? CategorySourceKey, string? CommoditySourceKey, string? PackageSourceKey, long? WooCategoryId, string? CategoryName, string? CategorySlug, long? WooCommodityId, string? CommodityName, string? CommoditySlug, long? QueueWooProductId, string? ProductName, string? ProductEnglishName, string? PackageGroup, string? PackageCode, string? PackageTitle, long? WooPackageId, decimal? UnitWeightKg, int? PacksPerCarton, decimal? PackagingPricePerPack, decimal? KgCashPrice, decimal? KgCreditPrice, decimal? SaleCashPrice, decimal? SaleCreditPrice, decimal? BuyCashPrice, decimal? BuyCreditPrice, string? Status, string? CatalogVisibility, string? BrandName, string? BrandEnglishName, string? ShortDescription, string? FullDescription, string? ImageUrl, string? GalleryJson);
    private sealed record PreparedSaleProduct(AllProductRow Source, SaleProductDraft Product, long GroupId);

    private sealed class WooSyncResult
    {
        public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAtUtc { get; set; }
        public int CategoriesSynced { get; set; }
        public int CategoriesFailed { get; set; }
        public int CommoditiesSynced { get; set; }
        public int CommoditiesFailed { get; set; }
        public int PackagesSynced { get; set; }
        public int PackagesFailed { get; set; }
        public int ProductsSynced { get; set; }
        public int ProductsFailed { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    private static IEnumerable<SaleProductDraft> BuildFinalSaleProducts(AllProductRow row, string priceUnit)
    {
        var productEnglishName = ResolveEnglishProductName(row.ProductEnglishName, row.ProductName, row.MainProductName, row.CategorySlug);
        var baseName = FirstNotEmpty(row.ProductName, productEnglishName, row.Sku, "product");
        var bulkWeight = row.UnitWeight ?? ExtractFirstDecimal(row.PackageName) ?? ExtractFirstDecimal(row.PackageOne) ?? 1m;
        var bulkPackageTitle = FirstNotEmpty(row.PackageName, row.PackageOne, $"{bulkWeight:0.###} kg");
        var bulkCode = BuildPackageCode(bulkPackageTitle, bulkWeight, "BULK");

        yield return BuildOne(
            row,
            priceUnit,
            productEnglishName,
            productName: CombineProductName(baseName, bulkPackageTitle),
            packageName: bulkPackageTitle,
            packagingGroup: "bulk",
            packageCode: bulkCode,
            unitWeight: bulkWeight,
            cartonQuantity: 1,
            packagingPricePerPackToman: 0m,
            sourceSuffix: bulkCode);

        if (row.HaveOtherPackage == 1)
        {
            var fee = row.PackagingPricePerPack.HasValue
                ? ConvertInputPriceToToman(row.PackagingPricePerPack, priceUnit) ?? 0m
                : DefaultRetailPackagingFeePerPack;

            yield return BuildOne(
                row,
                priceUnit,
                productEnglishName,
                productName: CombineProductName(baseName, "450 گرمی کارتن 12 عددی"),
                packageName: "450 گرمی",
                packagingGroup: "retail",
                packageCode: "450",
                unitWeight: 0.45m,
                cartonQuantity: 12,
                packagingPricePerPackToman: fee,
                sourceSuffix: "450");

            yield return BuildOne(
                row,
                priceUnit,
                productEnglishName,
                productName: CombineProductName(baseName, "900 گرمی کارتن 6 عددی"),
                packageName: "900 گرمی",
                packagingGroup: "retail",
                packageCode: "900",
                unitWeight: 0.90m,
                cartonQuantity: 6,
                packagingPricePerPackToman: fee,
                sourceSuffix: "900");
        }
    }

    private static SaleProductDraft BuildOne(
        AllProductRow row,
        string priceUnit,
        string productEnglishName,
        string productName,
        string packageName,
        string packagingGroup,
        string packageCode,
        decimal unitWeight,
        int cartonQuantity,
        decimal packagingPricePerPackToman,
        string sourceSuffix)
    {
        var kgCashToman = ConvertInputPriceToToman(row.SaleKgPriceCash, priceUnit);
        var kgCreditToman = ConvertInputPriceToToman(row.SaleKgPriceInstallment, priceUnit);
        var buyKgCashToman = ConvertInputPriceToToman(row.PurchaseKgPriceCash, priceUnit);
        var buyKgCreditToman = ConvertInputPriceToToman(row.PurchaseKgPriceInstallment, priceUnit);

        var saleCash = CalculateFinalPrice(kgCashToman, unitWeight, cartonQuantity, packagingPricePerPackToman, packagingGroup);
        var saleCredit = CalculateFinalPrice(kgCreditToman, unitWeight, cartonQuantity, packagingPricePerPackToman, packagingGroup);
        var buyCash = CalculateFinalPrice(buyKgCashToman, unitWeight, cartonQuantity, packagingPricePerPackToman, packagingGroup);
        var buyCredit = CalculateFinalPrice(buyKgCreditToman, unitWeight, cartonQuantity, packagingPricePerPackToman, packagingGroup);
        var status = NormalizeStatus(row.Status, saleCash, saleCredit);
        var brandSlug = BuildEnglishSlug(FirstNotEmpty(row.BrandEnglishName, row.BrandName));
        var slugBase = BuildEnglishSlug(FirstNotEmpty(productEnglishName, row.ProductSlug, row.Sku, row.CategorySlug, "kharbarchi-product"));
        var sku = BuildSku(row, productEnglishName, packageCode);
        var sourceHash = ComputeSha256(row.SourceRowHash + "|" + sourceSuffix);
        var finalSlug = BuildEnglishSlug(string.Join("-", new[] { slugBase, brandSlug, packageCode }.Where(x => !string.IsNullOrWhiteSpace(x) && x != "item")));

        return new SaleProductDraft(
            SourceRowHash: sourceHash,
            WooProductId: row.WooProductId,
            ProductName: productName,
            ProductEnglishName: CombineEnglishProductName(productEnglishName, packageCode),
            ProductSlug: finalSlug,
            Sku: sku,
            BrandName: row.BrandName,
            BrandEnglishName: row.BrandEnglishName,
            PackageName: packageName,
            PackagingGroup: packagingGroup,
            PackageCode: packageCode,
            UnitWeight: unitWeight,
            PacksPerCarton: cartonQuantity,
            CartonQuantity: cartonQuantity,
            PackagingPricePerPack: packagingPricePerPackToman,
            KgPriceCash: kgCashToman,
            KgPriceInstallment: kgCreditToman,
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
            // KHB-246: for carton-packaged products, the input price is the package price.
            // Kg price and packaging fee can be kept as metadata, but must not drive the final carton price.
            return Math.Round(cartonQuantity * kgPrice.Value, 0);
        }

        // Legacy "bulk" group means carton-weight sale: final price = kg price * unique carton weight.
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

    private static async Task UpsertSourceProductAsync(MySqlConnection connection, AllProductRow row, string productEnglishName, string priceUnit, CancellationToken cancellationToken)
    {
        var kgCashToman = ConvertInputPriceToToman(row.SaleKgPriceCash, priceUnit);
        var kgCreditToman = ConvertInputPriceToToman(row.SaleKgPriceInstallment, priceUnit);
        await ExecuteUpsertAsync(connection, @"
INSERT INTO khb_source_product
(SourceKey, SourceRowId, ProductName, ProductEnglishName, MainProductName, CategoryName, CategorySlug, BrandName, BrandEnglishName, PackageOne, UnitWeightKg, KgCashPrice, KgCreditPrice, RawJson, CreatedAtUtc, UpdatedAtUtc)
VALUES
(@SourceKey, @SourceRowId, @ProductName, @ProductEnglishName, @MainProductName, @CategoryName, @CategorySlug, @BrandName, @BrandEnglishName, @PackageOne, @UnitWeightKg, @KgCashPrice, @KgCreditPrice, @RawJson, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE ProductName = VALUES(ProductName), ProductEnglishName = VALUES(ProductEnglishName), MainProductName = VALUES(MainProductName), CategoryName = VALUES(CategoryName), CategorySlug = VALUES(CategorySlug), BrandName = VALUES(BrandName), BrandEnglishName = VALUES(BrandEnglishName), PackageOne = VALUES(PackageOne), UnitWeightKg = VALUES(UnitWeightKg), KgCashPrice = VALUES(KgCashPrice), KgCreditPrice = VALUES(KgCreditPrice), RawJson = VALUES(RawJson), UpdatedAtUtc = UTC_TIMESTAMP(6);",
            cancellationToken,
            ("@SourceKey", row.SourceRowHash),
            ("@SourceRowId", row.Id),
            ("@ProductName", row.ProductName),
            ("@ProductEnglishName", productEnglishName),
            ("@MainProductName", row.MainProductName),
            ("@CategoryName", row.CategoryName),
            ("@CategorySlug", BuildEnglishSlug(row.CategorySlug)),
            ("@BrandName", row.BrandName),
            ("@BrandEnglishName", row.BrandEnglishName),
            ("@PackageOne", row.PackageOne),
            ("@UnitWeightKg", row.UnitWeight),
            ("@KgCashPrice", kgCashToman),
            ("@KgCreditPrice", kgCreditToman),
            ("@RawJson", row.RawJson));
    }

    private async Task<int> ExecutePreparedPhaseAsync(
        MySqlConnection connection,
        KhbWorkflowJob job,
        IReadOnlyList<PreparedSaleProduct> rows,
        HashSet<string> blocked,
        string step,
        int startPercent,
        int endPercent,
        Func<MySqlConnection, PreparedSaleProduct, CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        var phaseErrors = 0;
        var initialErrors = job.ErrorCount;
        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            if (!blocked.Contains(row.Product.SourceRowHash))
            {
                try
                {
                    await operation(connection, row, cancellationToken);
                }
                catch (Exception ex)
                {
                    phaseErrors++;
                    blocked.Add(row.Product.SourceRowHash);
                    await _jobs.AddLogAsync(
                        job,
                        step,
                        "product",
                        row.Product.SourceRowHash,
                        row.Product.Sku,
                        "error",
                        ex.Message,
                        ex is WooCommerceProfileException woo ? woo.RequestUrl : null,
                        ex is WooCommerceProfileException wooError ? wooError.ResponseCode : null,
                        ex is WooCommerceProfileException wooBody ? wooBody.ResponseSummary : null,
                        cancellationToken);
                }
            }

            var percent = rows.Count == 0
                ? endPercent
                : startPercent + (int)Math.Round((index + 1) * (endPercent - startPercent) / (double)rows.Count);
            var processed = step == "Building WooCommerce sync queue"
                ? Math.Max(0, index + 1 - blocked.Count)
                : 0;
            await _jobs.SetPhaseAsync(
                job,
                step,
                percent,
                rows.Count,
                processed,
                processed,
                initialErrors + phaseErrors,
                job.DraftCount,
                job.SkippedCount,
                $"{step}: {index + 1} of {rows.Count}",
                cancellationToken);
        }

        return phaseErrors;
    }

    private static Task UpsertCategoryWorkflowAsync(MySqlConnection connection, PreparedSaleProduct row, CancellationToken cancellationToken)
    {
        var categorySlug = BuildEnglishSlug(FirstNotEmpty(row.Source.CategorySlug, row.Source.ProductEnglishName, row.Source.CategoryName));
        var categoryName = FirstNotEmpty(row.Source.CategoryName, row.Source.MainProductName, categorySlug);
        return ExecuteUpsertAsync(connection, @"
INSERT INTO khb_category_map (SourceKey, CategoryName, CategorySlug, WooCategoryId, CreatedAtUtc, UpdatedAtUtc)
VALUES (@SourceKey, @CategoryName, @CategorySlug, NULL, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE CategoryName = VALUES(CategoryName), CategorySlug = VALUES(CategorySlug), UpdatedAtUtc = UTC_TIMESTAMP(6);",
            cancellationToken,
            ("@SourceKey", $"cat:{categorySlug}"),
            ("@CategoryName", categoryName),
            ("@CategorySlug", categorySlug));
    }

    private static Task UpsertCommodityWorkflowAsync(MySqlConnection connection, PreparedSaleProduct row, CancellationToken cancellationToken)
    {
        var commoditySlug = BuildEnglishSlug(FirstNotEmpty(row.Source.MainProductSlug, row.Source.ProductEnglishName, row.Source.MainProductName));
        var commodityName = FirstNotEmpty(row.Source.MainProductName, row.Source.ProductName, commoditySlug);
        return ExecuteUpsertAsync(connection, @"
INSERT INTO khb_commodity (SourceKey, CommodityName, CommoditySlug, WooCommodityId, CreatedAtUtc, UpdatedAtUtc)
VALUES (@SourceKey, @CommodityName, @CommoditySlug, NULL, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE CommodityName = VALUES(CommodityName), CommoditySlug = VALUES(CommoditySlug), UpdatedAtUtc = UTC_TIMESTAMP(6);",
            cancellationToken,
            ("@SourceKey", $"commodity:{commoditySlug}"),
            ("@CommodityName", commodityName),
            ("@CommoditySlug", commoditySlug));
    }

    private static Task UpsertPackageWorkflowAsync(MySqlConnection connection, PreparedSaleProduct row, CancellationToken cancellationToken)
    {
        var product = row.Product;
        var packageSourceKey = $"pkg:{product.PackagingGroup}:{product.PackageCode}".ToLowerInvariant();
        return ExecuteUpsertAsync(connection, @"
INSERT INTO khb_package_type (SourceKey, PackageGroup, PackageCode, PackageTitle, UnitWeightKg, PacksPerCarton, PackagingPricePerPack, CreatedAtUtc, UpdatedAtUtc)
VALUES (@SourceKey, @PackageGroup, @PackageCode, @PackageTitle, @UnitWeightKg, @PacksPerCarton, @PackagingPricePerPack, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE PackageGroup = VALUES(PackageGroup), PackageCode = VALUES(PackageCode), PackageTitle = VALUES(PackageTitle), UnitWeightKg = VALUES(UnitWeightKg), PacksPerCarton = VALUES(PacksPerCarton), PackagingPricePerPack = VALUES(PackagingPricePerPack), UpdatedAtUtc = UTC_TIMESTAMP(6);",
            cancellationToken,
            ("@SourceKey", packageSourceKey),
            ("@PackageGroup", product.PackagingGroup),
            ("@PackageCode", product.PackageCode),
            ("@PackageTitle", product.PackageName),
            ("@UnitWeightKg", product.UnitWeight),
            ("@PacksPerCarton", product.PacksPerCarton),
            ("@PackagingPricePerPack", product.PackagingPricePerPack));
    }

    private static async Task UpsertFinalWorkflowAsync(MySqlConnection connection, PreparedSaleProduct row, CancellationToken cancellationToken)
    {
        var source = row.Source;
        var product = row.Product;
        var categorySlug = BuildEnglishSlug(FirstNotEmpty(source.CategorySlug, source.ProductEnglishName, source.CategoryName));
        var commoditySlug = BuildEnglishSlug(FirstNotEmpty(source.MainProductSlug, source.ProductEnglishName, source.MainProductName));
        var packageSourceKey = $"pkg:{product.PackagingGroup}:{product.PackageCode}".ToLowerInvariant();
        var payload = BuildQueuePayload(product);

        await UpsertSaleProductAsync(connection, row.GroupId, product, cancellationToken);
        await ExecuteUpsertAsync(connection, @"
INSERT INTO khb_product_final
(SourceKey, MainGroupId, CategorySourceKey, CommoditySourceKey, PackageSourceKey, ProductName, ProductEnglishName, ProductSlug, SKU, PackageGroup, PackageCode, SaleMode, PriceCalculationBasis, UnitWeightKg, PacksPerCarton, PackagingPricePerPack, KgCashPrice, KgCreditPrice, SaleCashPrice, SaleCreditPrice, BuyCashPrice, BuyCreditPrice, Status, CatalogVisibility, WooPayloadJson, CreatedAtUtc, UpdatedAtUtc)
VALUES
(@SourceKey, @MainGroupId, @CategorySourceKey, @CommoditySourceKey, @PackageSourceKey, @ProductName, @ProductEnglishName, @ProductSlug, @SKU, @PackageGroup, @PackageCode, @SaleMode, @PriceCalculationBasis, @UnitWeightKg, @PacksPerCarton, @PackagingPricePerPack, @KgCashPrice, @KgCreditPrice, @SaleCashPrice, @SaleCreditPrice, @BuyCashPrice, @BuyCreditPrice, @Status, @CatalogVisibility, @WooPayloadJson, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE MainGroupId = VALUES(MainGroupId), CategorySourceKey = VALUES(CategorySourceKey), CommoditySourceKey = VALUES(CommoditySourceKey), PackageSourceKey = VALUES(PackageSourceKey), ProductName = VALUES(ProductName), ProductEnglishName = VALUES(ProductEnglishName), ProductSlug = VALUES(ProductSlug), SKU = VALUES(SKU), PackageGroup = VALUES(PackageGroup), PackageCode = VALUES(PackageCode), SaleMode = VALUES(SaleMode), PriceCalculationBasis = VALUES(PriceCalculationBasis), UnitWeightKg = VALUES(UnitWeightKg), PacksPerCarton = VALUES(PacksPerCarton), PackagingPricePerPack = VALUES(PackagingPricePerPack), KgCashPrice = VALUES(KgCashPrice), KgCreditPrice = VALUES(KgCreditPrice), SaleCashPrice = VALUES(SaleCashPrice), SaleCreditPrice = VALUES(SaleCreditPrice), BuyCashPrice = VALUES(BuyCashPrice), BuyCreditPrice = VALUES(BuyCreditPrice), Status = VALUES(Status), CatalogVisibility = VALUES(CatalogVisibility), WooPayloadJson = VALUES(WooPayloadJson), UpdatedAtUtc = UTC_TIMESTAMP(6);",
            cancellationToken,
            ("@SourceKey", product.SourceRowHash),
            ("@MainGroupId", row.GroupId),
            ("@CategorySourceKey", $"cat:{categorySlug}"),
            ("@CommoditySourceKey", $"commodity:{commoditySlug}"),
            ("@PackageSourceKey", packageSourceKey),
            ("@ProductName", product.ProductName),
            ("@ProductEnglishName", product.ProductEnglishName),
            ("@ProductSlug", product.ProductSlug),
            ("@SKU", product.Sku),
            ("@PackageGroup", product.PackagingGroup),
            ("@PackageCode", product.PackageCode),
            ("@SaleMode", GetSaleMode(product.PackagingGroup)),
            ("@PriceCalculationBasis", GetPriceCalculationBasis(product.PackagingGroup)),
            ("@UnitWeightKg", product.UnitWeight),
            ("@PacksPerCarton", product.PacksPerCarton),
            ("@PackagingPricePerPack", product.PackagingPricePerPack),
            ("@KgCashPrice", product.KgPriceCash),
            ("@KgCreditPrice", product.KgPriceInstallment),
            ("@SaleCashPrice", product.SalePriceCash),
            ("@SaleCreditPrice", product.SalePriceInstallment),
            ("@BuyCashPrice", product.PurchasePriceCash),
            ("@BuyCreditPrice", product.PurchasePriceInstallment),
            ("@Status", product.Status),
            ("@CatalogVisibility", product.Status == "publish" ? "visible" : "hidden"),
            ("@WooPayloadJson", payload));
        await RegisterPriceHistoryAsync(connection, product, cancellationToken);
        await ExecuteUpsertAsync(connection, @"
INSERT INTO khb_product_change_log (ProductId, ChangeType, Summary, Payload, CreatedAtUtc)
SELECT Id, 'upsert', @Summary, @Payload, UTC_TIMESTAMP(6)
FROM khb_product_final
WHERE SourceKey = @SourceKey
LIMIT 1;",
            cancellationToken,
            ("@Summary", $"CSV workflow upsert for SKU {product.Sku}"),
            ("@Payload", payload),
            ("@SourceKey", product.SourceRowHash));
    }

    private static Task QueueWorkflowAsync(
        MySqlConnection connection,
        PreparedSaleProduct row,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var product = row.Product;
        return ExecuteUpsertAsync(connection, @"
INSERT INTO khb_product_update_queue
(SourceKey, EntityType, QueueStatus, ActionType, SKU, ProductSlug, WooPayloadJson, JobId, TryCount, CreatedAtUtc, UpdatedAtUtc)
VALUES
(@SourceKey, 'product', 'pending', 'upsert', @SKU, @ProductSlug, @WooPayloadJson, @JobId, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE EntityType = 'product', QueueStatus = 'pending', ActionType = 'upsert', SKU = VALUES(SKU), ProductSlug = VALUES(ProductSlug), WooPayloadJson = VALUES(WooPayloadJson), JobId = VALUES(JobId), TryCount = 0, LastError = NULL, UpdatedAtUtc = UTC_TIMESTAMP(6);",
            cancellationToken,
            ("@SourceKey", product.SourceRowHash),
            ("@SKU", product.Sku),
            ("@ProductSlug", product.ProductSlug),
            ("@WooPayloadJson", BuildQueuePayload(product)),
            ("@JobId", jobId.ToString()));
    }

    private static string BuildQueuePayload(SaleProductDraft product) =>
        JsonSerializer.Serialize(new
        {
            name = product.ProductName,
            slug = product.ProductSlug,
            sku = product.Sku,
            regular_price = product.SalePriceInstallment?.ToString("0", CultureInfo.InvariantCulture),
            sale_price = string.Empty,
            status = product.Status,
            catalog_visibility = product.Status == "publish" ? "visible" : "hidden",
            meta_data = new[]
            {
                new { key = "_kharbarchi_sale_cash_price", value = product.SalePriceCash?.ToString("0", CultureInfo.InvariantCulture) },
                new { key = "_kharbarchi_sale_credit_price", value = product.SalePriceInstallment?.ToString("0", CultureInfo.InvariantCulture) },
                new { key = "_kharbarchi_buy_cash_price", value = product.PurchasePriceCash?.ToString("0", CultureInfo.InvariantCulture) },
                new { key = "_kharbarchi_buy_credit_price", value = product.PurchasePriceInstallment?.ToString("0", CultureInfo.InvariantCulture) },
                new { key = "_kharbarchi_package_group", value = (string?)product.PackagingGroup },
                new { key = "_kharbarchi_package_code", value = (string?)product.PackageCode },
                new { key = "_khb_sale_mode", value = (string?)GetSaleMode(product.PackagingGroup) },
                new { key = "_khb_price_calculation_basis", value = (string?)GetPriceCalculationBasis(product.PackagingGroup) },
                new { key = "_kharbarchi_packs_per_carton", value = (string?)product.PacksPerCarton.ToString(CultureInfo.InvariantCulture) },
                new { key = "_kharbarchi_unit_weight_kg", value = (string?)product.UnitWeight.ToString("0.####", CultureInfo.InvariantCulture) }
            }
        }, JsonOptions);

#if false
    // Legacy destructive helpers are intentionally excluded from compilation.
    // Schema ownership belongs exclusively to reviewed EF Core migrations.
    private static Task ResetGeneratedProductTablesAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        // KHB-SAFE: destructive reset is disabled. Workflow processing is upsert-only.
        return Task.CompletedTask;
    }

    private static Task TruncateTableIfExistsAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        // KHB-SAFE: table truncation is forbidden by project rules.
        return Task.CompletedTask;
    }

    private static async Task<bool> TableExistsAsync(MySqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @TableName;";
        Add(command, "@TableName", tableName);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result, CultureInfo.InvariantCulture) > 0;
    }
#endif

    private static async Task ExecuteUpsertAsync(MySqlConnection connection, string sql, CancellationToken cancellationToken, params (string Name, object? Value)[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var parameter in parameters)
        {
            Add(command, parameter.Name, parameter.Value);
        }
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task RegisterPriceHistoryAsync(MySqlConnection connection, SaleProductDraft product, CancellationToken cancellationToken)
    {
        var productType = GetSaleMode(product.PackagingGroup);
        await RegisterPriceHistoryValueAsync(connection, product, productType, "sale_cash", product.SalePriceCash, cancellationToken);
        await RegisterPriceHistoryValueAsync(connection, product, productType, "sale_credit", product.SalePriceInstallment, cancellationToken);
        await RegisterPriceHistoryValueAsync(connection, product, productType, "buy_cash", product.PurchasePriceCash, cancellationToken);
        await RegisterPriceHistoryValueAsync(connection, product, productType, "buy_credit", product.PurchasePriceInstallment, cancellationToken);

        // Informational values: they may be stored for control/reporting, but carton-packaged final pricing must not use them.
        await RegisterPriceHistoryValueAsync(connection, product, productType, "kg_cash_info", product.KgPriceCash, cancellationToken);
        await RegisterPriceHistoryValueAsync(connection, product, productType, "kg_credit_info", product.KgPriceInstallment, cancellationToken);
        await RegisterPriceHistoryValueAsync(connection, product, productType, "packaging_fee_info", product.PackagingPricePerPack > 0 ? product.PackagingPricePerPack : null, cancellationToken);
    }

    private static async Task RegisterPriceHistoryValueAsync(
        MySqlConnection connection,
        SaleProductDraft product,
        string productType,
        string priceType,
        decimal? priceAmount,
        CancellationToken cancellationToken)
    {
        if (!priceAmount.HasValue || priceAmount.Value <= 0)
        {
            return;
        }

        await using var current = connection.CreateCommand();
        current.CommandText = @"
SELECT Id, PriceAmount
FROM khb_product_price_history
WHERE ProductSourceKey = @ProductSourceKey
  AND ProductType = @ProductType
  AND PriceType = @PriceType
  AND IsCurrent = 1
ORDER BY ValidFromUtc DESC, Id DESC
LIMIT 1;";
        Add(current, "@ProductSourceKey", product.SourceRowHash);
        Add(current, "@ProductType", productType);
        Add(current, "@PriceType", priceType);

        long? currentId = null;
        decimal? currentPrice = null;
        await using (var reader = await current.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                currentId = reader.GetInt64(0);
                currentPrice = ReadNullableDecimal(reader, 1);
            }
        }

        if (currentId.HasValue && currentPrice.HasValue && currentPrice.Value == priceAmount.Value)
        {
            await ExecuteUpsertAsync(connection, @"
UPDATE khb_product_price_history
SET ProductName = @ProductName,
    SKU = @SKU,
    PackageGroup = @PackageGroup,
    PackageCode = @PackageCode,
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE Id = @Id;", cancellationToken,
                ("@ProductName", product.ProductName),
                ("@SKU", product.Sku),
                ("@PackageGroup", product.PackagingGroup),
                ("@PackageCode", product.PackageCode),
                ("@Id", currentId.Value));
            return;
        }

        if (currentId.HasValue)
        {
            await ExecuteUpsertAsync(connection, @"
UPDATE khb_product_price_history
SET ValidToUtc = UTC_TIMESTAMP(6), IsCurrent = 0, UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE Id = @Id;", cancellationToken,
                ("@Id", currentId.Value));
        }

        await ExecuteUpsertAsync(connection, @"
INSERT INTO khb_product_price_history
(
    ProductSourceKey,
    ProductName,
    SKU,
    ProductType,
    PackageGroup,
    PackageCode,
    PriceType,
    PriceAmount,
    CurrencyCode,
    ValidFromUtc,
    ValidToUtc,
    IsCurrent,
    CreatedAtUtc,
    UpdatedAtUtc
)
VALUES
(
    @ProductSourceKey,
    @ProductName,
    @SKU,
    @ProductType,
    @PackageGroup,
    @PackageCode,
    @PriceType,
    @PriceAmount,
    'TOMAN',
    UTC_TIMESTAMP(6),
    NULL,
    1,
    UTC_TIMESTAMP(6),
    UTC_TIMESTAMP(6)
);", cancellationToken,
            ("@ProductSourceKey", product.SourceRowHash),
            ("@ProductName", product.ProductName),
            ("@SKU", product.Sku),
            ("@ProductType", productType),
            ("@PackageGroup", product.PackagingGroup),
            ("@PackageCode", product.PackageCode),
            ("@PriceType", priceType),
            ("@PriceAmount", priceAmount.Value));
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
    SaleMode,
    PriceCalculationBasis,
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
    @SaleMode,
    @PriceCalculationBasis,
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
    SaleMode = VALUES(SaleMode),
    PriceCalculationBasis = VALUES(PriceCalculationBasis),
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
        Add(command, "@SaleMode", GetSaleMode(row.PackagingGroup));
        Add(command, "@PriceCalculationBasis", GetPriceCalculationBasis(row.PackagingGroup));
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

    private static Task ValidateAllProductTableAsync(
        MySqlConnection connection,
        string tableName,
        CancellationToken cancellationToken) =>
        ValidateRequiredSchemaAsync(
            connection,
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [tableName] =
                [
                    "Id", "ImportBatchId", "SourceRowNumber", "SourceRowHash", "RawJson",
                    "MainProductName", "MainProductSlug", "GroupName", "CategoryName", "CategorySlug",
                    "ProductName", "ProductEnglishName", "ProductSlug", "SKU", "BrandName",
                    "BrandEnglishName", "PackageName", "UnitWeight", "PacksPerCarton", "CartonQuantity",
                    "PackagingPricePerPack", "SalePriceCash", "SalePriceInstallment", "PurchasePriceCash",
                    "PurchasePriceInstallment", "ShortDescription", "FullDescription", "ImageUrl",
                    "GalleryJson", "Status", "WooProductId", "HaveOtherPackage", "PackageOne",
                    "CreatedAtUtc", "UpdatedAtUtc"
                ]
            },
            cancellationToken);

    private static Task ValidateProductManagementTablesAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken) =>
        ValidateRequiredSchemaAsync(
            connection,
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["khb_product_main_groups"] = ["Id", "SourceKey", "Name", "MainProductName", "MainProductSlug", "CategoryName", "EnTaxonomic", "CategorySlug"],
                ["khb_sale_products"] = ["Id", "MainGroupId", "SourceRowHash", "WooProductId", "ProductName", "ProductSlug", "SKU", "PackageName", "PackagingGroup", "PackageCode", "SaleMode", "PriceCalculationBasis", "Status"],
                ["khb_source_product"] = ["Id", "SourceKey", "SourceRowId", "ProductName", "ProductEnglishName", "MainProductName", "CategoryName", "CategorySlug", "BrandName", "BrandEnglishName", "UnitWeightKg", "KgCashPrice", "KgCreditPrice"],
                ["khb_category_map"] = ["Id", "SourceKey", "CategoryName", "CategorySlug", "WooCategoryId"],
                ["khb_commodity"] = ["Id", "SourceKey", "CommodityName", "CommoditySlug", "WooCommodityId"],
                ["khb_package_type"] = ["Id", "SourceKey", "PackageGroup", "PackageCode", "PackageTitle", "UnitWeightKg", "PacksPerCarton", "PackagingPricePerPack", "WooPackageId"],
                ["khb_product_final"] = ["Id", "SourceKey", "MainGroupId", "CategorySourceKey", "CommoditySourceKey", "PackageSourceKey", "ProductName", "ProductEnglishName", "ProductSlug", "SKU", "WooProductId", "SaleMode", "PriceCalculationBasis", "Status", "WooPayloadJson"],
                ["khb_product_update_queue"] = ["Id", "SourceKey", "EntityType", "QueueStatus", "ActionType", "SKU", "ProductSlug", "WooProductId", "WooPayloadJson", "LastError", "JobId", "TryCount"],
                ["khb_product_price_history"] = ["Id", "ProductSourceKey", "ProductName", "SKU", "ProductType", "PriceType", "PriceAmount", "ValidFromUtc", "IsCurrent"],
                ["khb_product_change_log"] = ["Id", "ProductId", "ChangeType", "CreatedAtUtc"],
                ["khb_workflow_jobs"] = ["Id", "JobId", "Type", "Status", "CurrentStep", "ProgressPercent"],
                ["khb_workflow_job_logs"] = ["Id", "JobId", "StepName", "Status", "CreatedAtUtc"],
                ["khb_woocommerce_connection_profiles"] = ["Id", "ProfileName", "EnvironmentType", "BaseUrl", "ProtectedConsumerSecret", "VerifySsl", "IsActive"]
            },
            cancellationToken);

    private static async Task ValidateRequiredSchemaAsync(
        MySqlConnection connection,
        IReadOnlyDictionary<string, string[]> requirements,
        CancellationToken cancellationToken)
    {
        var tableParameters = requirements.Keys.Select((_, index) => $"@Table{index}").ToArray();
        await using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME IN ({string.Join(", ", tableParameters)});";
        var parameterIndex = 0;
        foreach (var table in requirements.Keys)
        {
            Add(command, tableParameters[parameterIndex++], table);
        }

        var actual = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var table = reader.GetString(0);
                var column = reader.GetString(1);
                if (!actual.TryGetValue(table, out var columns))
                {
                    columns = new HashSet<string>(StringComparer.Ordinal);
                    actual[table] = columns;
                }
                columns.Add(column);
            }
        }

        var missing = new List<string>();
        foreach (var requirement in requirements)
        {
            if (!actual.TryGetValue(requirement.Key, out var columns))
            {
                missing.Add($"table:{requirement.Key}");
                continue;
            }

            missing.AddRange(requirement.Value
                .Where(column => !columns.Contains(column))
                .Select(column => $"{requirement.Key}.{column}"));
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                "Canonical KHB schema is incomplete. Apply the reviewed EF Core migration first. Missing: "
                + string.Join(", ", missing));
        }
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
        if (!string.Equals(name, DefaultTableName, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Only the canonical table '{DefaultTableName}' is supported.");
        }
        return DefaultTableName;
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


    private static string NormalizePriceUnit(string? value)
    {
        var unit = (value ?? "rial").Trim().ToLowerInvariant();
        return unit is "toman" or "tomans" or "تومان" ? "toman" : "rial";
    }

    private static decimal? ConvertInputPriceToToman(decimal? value, string priceUnit)
    {
        if (!value.HasValue) return null;
        return NormalizePriceUnit(priceUnit) == "rial"
            ? Math.Round(value.Value / 10m, 2)
            : Math.Round(value.Value, 2);
    }

    private static string ResolveEnglishProductName(string? explicitEnglish, string? productName, string? mainProductName, string? categorySlug)
    {
        var explicitValue = FirstNotEmpty(explicitEnglish);
        if (!string.IsNullOrWhiteSpace(explicitValue) && BuildEnglishSlug(explicitValue) != "kharbarchi-product")
        {
            return BuildEnglishSlug(explicitValue).Replace('-', ' ');
        }

        var key = NormalizePersianKey(FirstNotEmpty(productName, mainProductName));
        if (HardcodedProductEnglishNames.TryGetValue(key, out var exact)) return exact;

        var phrase = FirstNotEmpty(productName, mainProductName);
        var translated = TranslatePersianPhraseToEnglish(phrase);
        if (!string.IsNullOrWhiteSpace(translated)) return translated;

        var fromCategory = BuildEnglishSlug(categorySlug);
        return fromCategory == "kharbarchi-product" ? "kharbarchi product" : fromCategory.Replace('-', ' ');
    }

    private static string CombineEnglishProductName(string productEnglishName, string packageCode)
    {
        var packageTitle = packageCode switch
        {
            "450" => "450g carton",
            "900" => "900g carton",
            _ when packageCode.StartsWith("B", StringComparison.OrdinalIgnoreCase) => packageCode.Replace('_', '.').ToLowerInvariant(),
            _ => packageCode.ToLowerInvariant()
        };
        return FirstNotEmpty(productEnglishName, "kharbarchi product") + " " + packageTitle;
    }

    private static string TranslatePersianPhraseToEnglish(string? value)
    {
        var text = NormalizePersianKey(value);
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var replacements = new (string Persian, string English)[]
        {
            ("لوبیا چشم بلبلی", "black eyed peas"),
            ("لوبیا قرمز", "red kidney beans"),
            ("لوبیا سفید", "white beans"),
            ("لوبیا چیتی", "pinto beans"),
            ("لوبیا کرم", "cream beans"),
            ("نخود فلافلی", "falafel chickpeas"),
            ("نخود", "chickpeas"),
            ("لپه", "yellow split peas"),
            ("عدس درشت", "large lentils"),
            ("عدس ریز", "small lentils"),
            ("دال عدس", "split red lentils"),
            ("ماش", "mung beans"),
            ("ذرت", "corn"),
            ("زنجان", "zanjan"),
            ("خمین", "khomein"),
            ("کرمانشاه", "kermanshah"),
            ("ماداگاسکار", "madagascar"),
            ("آذر شهر", "azarshahr"),
            ("روسیه", "russia"),
            ("روس", "russian"),
            ("کانادا", "canada"),
            ("ترک", "turkish"),
            ("برزیلی", "brazilian"),
            ("آرژانتینی", "argentinian"),
            ("آرژانتین", "argentina"),
            ("اتیوپی", "ethiopia"),
            ("ازبکستان", "uzbekistan"),
            ("ازبک", "uzbek"),
            ("قرقیزستان", "kyrgyzstan"),
            ("افغان", "afghan"),
            ("ایرانی", "iranian"),
            ("چینی", "chinese"),
            ("لوکس", "luxury"),
            ("کیسه شیشه ایی", "clear bag"),
            ("کیسه شیشه ای", "clear bag"),
            ("کراپ آخر 2024", "crop 2024"),
            ("کراپ 2025", "crop 2025"),
            ("کراپ آخر", "latest crop"),
            ("عروس", "bride"),
            ("باب لپه", "split pea grade"),
            ("سه خان", "three size"),
            ("دو خان", "two size"),
            ("متوسط", "medium")
        };

        var parts = new List<string>();
        foreach (var pair in replacements)
        {
            if (text.Contains(pair.Persian, StringComparison.Ordinal))
            {
                parts.Add(pair.English);
                text = text.Replace(pair.Persian, " ", StringComparison.Ordinal);
            }
        }

        return string.Join(' ', parts.Distinct()).Trim();
    }

    private static string NormalizePersianKey(string? value)
        => ProductRowMapper.NormalizeDigitsPublic(value ?? string.Empty)
            .Replace('ي', 'ی')
            .Replace('ك', 'ک')
            .Replace("\u200c", " ", StringComparison.Ordinal)
            .Replace("\u00a0", " ", StringComparison.Ordinal)
            .Trim();

    private static readonly Dictionary<string, string> HardcodedProductEnglishNames = new(StringComparer.Ordinal)
    {
        [NormalizePersianKey("لوبیا چیتی زنجان لوکس")] = "luxury zanjan pinto beans",
        [NormalizePersianKey("لوبیا چیتی زنجان")] = "zanjan pinto beans",
        [NormalizePersianKey("لوبیا چیتی خمین لوکس")] = "luxury khomein pinto beans",
        [NormalizePersianKey("لوبیا چیتی خمین")] = "khomein pinto beans",
        [NormalizePersianKey("لوبیا قرمز زنجان لوکس")] = "luxury zanjan red kidney beans",
        [NormalizePersianKey("لوبیا قرمز خمین لوکس")] = "luxury khomein red kidney beans",
        [NormalizePersianKey("لوبیا سفید خمین لوکس")] = "luxury khomein white beans",
        [NormalizePersianKey("لوبیا چشم بلبلی ماداگاسکار 25 کیلویی")] = "madagascar black eyed peas",
        [NormalizePersianKey("لوبیا چشم بلبلی ماداگاسکار 15 کیلویی")] = "madagascar black eyed peas",
        [NormalizePersianKey("نخود سه خان کرمانشاه لوکس")] = "luxury kermanshah chickpeas three size",
        [NormalizePersianKey("نخود سه خان کرمانشاه کیسه شیشه ایی")] = "kermanshah chickpeas three size clear bag",
        [NormalizePersianKey("نخود دو خان کرمانشاه کیسه شیشه ایی")] = "kermanshah chickpeas two size clear bag",
        [NormalizePersianKey("لپه آذر شهر (نخود هندی)")] = "azarshahr yellow split peas",
        [NormalizePersianKey("عدس درشت روس لوکس")] = "luxury russian large lentils",
        [NormalizePersianKey("عدس درشت روس")] = "russian large lentils",
        [NormalizePersianKey("عدس درشت کانادا کراپ آخر 2024")] = "canada large lentils crop 2024",
        [NormalizePersianKey("عدس درشت کانادا کراپ  2025 لوکس")] = "luxury canada large lentils crop 2025",
        [NormalizePersianKey("عدس ریز کانادا کراپ  2025 لوکس")] = "luxury canada small lentils crop 2025",
        [NormalizePersianKey("ماش افغان ")] = "afghan mung beans",
        [NormalizePersianKey("ماش افغان لوکس")] = "luxury afghan mung beans",
        [NormalizePersianKey("لپه متوسط روس")] = "russian medium yellow split peas",
        [NormalizePersianKey("لوبیا قرمز قرقیزستان")] = "kyrgyzstan red kidney beans",
        [NormalizePersianKey("لوبیا چیتی ازبکستان")] = "uzbekistan pinto beans",
        [NormalizePersianKey("عدس درشت کانادا ساسکن کراپ 2025")] = "saskan canada large lentils crop 2025",
        [NormalizePersianKey("نخود فلافلی ایرانی بار لوکس")] = "luxury iranian falafel chickpeas",
        [NormalizePersianKey("دال عدس ")] = "split red lentils",
        [NormalizePersianKey("نخود فلافلی روس")] = "russian falafel chickpeas",
        [NormalizePersianKey("نخود 7/8 ترک")] = "turkish chickpeas 7 8",
        [NormalizePersianKey("لوبیا کرم")] = "cream beans",
        [NormalizePersianKey("لوبیا چیتی عروس")] = "bride pinto beans",
        [NormalizePersianKey("ماش ونزوئلا")] = "venezuela mung beans",
        [NormalizePersianKey("ماش ازبک")] = "uzbek mung beans",
        [NormalizePersianKey("لوبیا چشم بلبلی ایرانی")] = "iranian black eyed peas",
        [NormalizePersianKey("ذرت ترک")] = "turkish corn",
        [NormalizePersianKey("ذرت برزیلی")] = "brazilian corn",
        [NormalizePersianKey("ذرت آرژانتین")] = "argentina corn",
        [NormalizePersianKey("ذرت اتیوپی")] = "ethiopia corn",
        [NormalizePersianKey("لوبیا چیتی کانادایی")] = "canadian pinto beans",
        [NormalizePersianKey("لوبیا چیتی چینی")] = "chinese pinto beans",
        [NormalizePersianKey("لوبیا چیتی آرژانتینی")] = "argentinian pinto beans",
        [NormalizePersianKey("نخود فلافلی یا باب لپه")] = "falafel chickpeas split pea grade",
        [NormalizePersianKey("عدس ریز کانادا کراپ آخر 2024")] = "canada small lentils crop 2024"
    };

    private static string BuildSku(AllProductRow row, string productEnglishName, string packageCode)
    {
        var p = Abbrev(productEnglishName, "KH", 2);
        var c = Abbrev(row.CategorySlug, "CT", 2);
        var b = Abbrev(row.BrandEnglishName, "BR", 2);
        return $"{p}_{c}_{b}_{packageCode}_{row.Id:000}".ToUpperInvariant();
    }

    private static string Abbrev(string? english, string? fallback, int length)
    {
        var source = FirstNotEmpty(english, fallback, "XX");
        var ascii = new string(source.Normalize(NormalizationForm.FormD)
            .Where(ch => ch < 128 && char.IsLetterOrDigit(ch))
            .ToArray())
            .ToUpperInvariant();
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

    private static string BuildEnglishSlug(string? value)
    {
        var text = ProductRowMapper.NormalizeDigitsPublic(value ?? string.Empty)
            .Replace('‑', '-')
            .Replace('–', '-')
            .Replace('—', '-')
            .Normalize(NormalizationForm.FormD)
            .ToLowerInvariant();
        var builder = new StringBuilder();
        foreach (var ch in text)
        {
            if (ch < 128 && char.IsLetterOrDigit(ch)) builder.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch is '_' or '-' or '/' or '\\') builder.Append('-');
        }
        var slug = builder.ToString().Trim('-');
        while (slug.Contains("--", StringComparison.Ordinal)) slug = slug.Replace("--", "-", StringComparison.Ordinal);
        return string.IsNullOrWhiteSpace(slug) ? "kharbarchi-product" : slug;
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

    private static string? ReadString(System.Data.Common.DbDataReader reader, string columnName) => ReadString(reader, reader.GetOrdinal(columnName));
    private static decimal? ReadNullableDecimal(System.Data.Common.DbDataReader reader, string columnName) => ReadNullableDecimal(reader, reader.GetOrdinal(columnName));
    private static int? ReadNullableInt(System.Data.Common.DbDataReader reader, string columnName) => ReadNullableInt(reader, reader.GetOrdinal(columnName));
    private static long? ReadNullableLong(System.Data.Common.DbDataReader reader, string columnName) => ReadNullableLong(reader, reader.GetOrdinal(columnName));

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public sealed class SourceRowDto
    {
        public long Id { get; set; }
        public int? SourceRowNumber { get; set; }
        public string? MainProductName { get; set; }
        public string? MainProductSlug { get; set; }
        public string? GroupName { get; set; }
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }
        public string? ProductName { get; set; }
        public string? ProductEnglishName { get; set; }
        public string? ProductSlug { get; set; }
        public string? Sku { get; set; }
        public string? BrandName { get; set; }
        public string? BrandEnglishName { get; set; }
        public string? PackageName { get; set; }
        public decimal? UnitWeight { get; set; }
        public int? CartonQuantity { get; set; }
        public decimal? PackagingPricePerPack { get; set; }
        public decimal? KgCashPrice { get; set; }
        public decimal? KgCreditPrice { get; set; }
        public decimal? PurchaseKgCashPrice { get; set; }
        public decimal? PurchaseKgCreditPrice { get; set; }
        public string? ShortDescription { get; set; }
        public string? FullDescription { get; set; }
        public string? ImageUrl { get; set; }
        public string? Status { get; set; }
        public int? HaveOtherPackage { get; set; }
        public string? PackageOne { get; set; }
        public string? UpdatedAtUtc { get; set; }
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
            if (string.IsNullOrWhiteSpace(englishName)) englishName = ResolveEnglishProductName(null, productName, FirstNotEmpty(main, categoryName), taxonomicSlug);

            var kgCash = GetDecimal(row, "KgPriceCash", "SaleKgPriceCash", "cash_price", "Cash_Price", "cash_sale_price", "قیمت کیلو نقد", "قیمت نقد کیلویی", "قیمت فروش نقد", "فروش نقد", "قیمت نقد");
            var kgCredit = GetDecimal(row, "KgPriceInstallment", "SaleKgPriceInstallment", "credit_price", "Credit_Price", "installment_sale_price", "قیمت کیلو شرایطی", "قیمت شرایطی کیلویی", "قیمت فروش شرایطی", "فروش شرایطی");
            var buyKgCash = GetDecimal(row, "PurchaseKgPriceCash", "BuyKgPriceCash", "purchase_cash_price", "قیمت خرید نقد", "خرید نقد", "قیمت خرید نقد کیلویی");
            var buyKgCredit = GetDecimal(row, "PurchaseKgPriceInstallment", "BuyKgPriceInstallment", "purchase_credit_price", "قیمت خرید شرایطی", "خرید شرایطی", "قیمت خرید شرایطی کیلویی");
            var packagingFee = GetDecimal(row, "PackagingPricePerPack", "PackageFee", "PackagingFee", "Package_Price", "قیمت بسته بندی", "هزینه بسته بندی", "قیمت بسته‌بندی");
            var brandEnglish = Get(row, "Brand_En", "BrandEn", "BrandEnglishName", "BrandEnglish", "برند انگلیسی");
            var englishSlug = BuildEnglishSlug(FirstNotEmpty(englishName, taxonomicSlug, brandEnglish, sku, productName));

            return new ProductMappedRow(
                MainProductName: FirstNotEmpty(main, categoryName),
                MainProductSlug: BuildEnglishSlug(FirstNotEmpty(englishName, taxonomicSlug, main, categoryName)),
                GroupName: Get(row, "GroupName", "Group", "terms", "Base_terms", "گروه"),
                CategoryName: categoryName,
                CategorySlug: taxonomicSlug,
                ProductName: productName,
                ProductEnglishName: englishName,
                ProductSlug: FirstNotEmpty(slug, englishSlug),
                Sku: sku,
                BrandName: Get(row, "Brand", "BrandName", "برند"),
                BrandEnglishName: brandEnglish,
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

        private static decimal? ToToman(decimal? value)
        {
            if (!value.HasValue) return null;
            return Math.Round(value.Value / 10m, 2);
        }

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
