namespace Kharbarchi.Server.Models;

public sealed class AllProductWithProcess
{
    public long Id { get; set; }
    public string? ImportBatchId { get; set; }
    public int? SourceRowNumber { get; set; }
    public string SourceRowHash { get; set; } = string.Empty;
    public string? RawJson { get; set; }
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
    public int? PacksPerCarton { get; set; }
    public int? CartonQuantity { get; set; }
    public decimal? PackagingPricePerPack { get; set; }
    public decimal? SalePriceCash { get; set; }
    public decimal? SalePriceInstallment { get; set; }
    public decimal? PurchasePriceCash { get; set; }
    public decimal? PurchasePriceInstallment { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? ImageUrl { get; set; }
    public string? GalleryJson { get; set; }
    public string? Status { get; set; }
    public long? WooProductId { get; set; }
    public bool? HaveOtherPackage { get; set; }
    public string? PackageOne { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class KhbProductMainGroup
{
    public long Id { get; set; }
    public string? MainProductName { get; set; }
    public string? MainProductSlug { get; set; }
    public string? CategoryName { get; set; }
    public string? EnTaxonomic { get; set; }
    public string? CategorySlug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string? SourceKey { get; set; }
    public string? Name { get; set; }
}

public sealed class KhbSaleProduct
{
    public long Id { get; set; }
    public long? MainGroupId { get; set; }
    public string SourceRowHash { get; set; } = string.Empty;
    public long? WooProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductEnglishName { get; set; }
    public string? ProductSlug { get; set; }
    public string? Sku { get; set; }
    public string? BrandName { get; set; }
    public string? BrandEnglishName { get; set; }
    public string? PackageName { get; set; }
    public string? PackagingGroup { get; set; }
    public string? PackageCode { get; set; }
    public decimal? UnitWeight { get; set; }
    public int? PacksPerCarton { get; set; }
    public int? CartonQuantity { get; set; }
    public decimal? PackagingPricePerPack { get; set; }
    public decimal? KgPriceCash { get; set; }
    public decimal? KgPriceInstallment { get; set; }
    public decimal? SalePriceCash { get; set; }
    public decimal? SalePriceInstallment { get; set; }
    public decimal? PurchasePriceCash { get; set; }
    public decimal? PurchasePriceInstallment { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? ImageUrl { get; set; }
    public string? GalleryJson { get; set; }
    public string Status { get; set; } = "draft";
    public string? RawJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string? SaleMode { get; set; }
    public string? PriceCalculationBasis { get; set; }
}

public sealed class KhbSourceProduct
{
    public long Id { get; set; }
    public string SourceKey { get; set; } = string.Empty;
    public long? SourceRowId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductEnglishName { get; set; }
    public string? MainProductName { get; set; }
    public string? CategoryName { get; set; }
    public string? CategorySlug { get; set; }
    public string? BrandName { get; set; }
    public string? BrandEnglishName { get; set; }
    public string? PackageOne { get; set; }
    public decimal? UnitWeightKg { get; set; }
    public decimal? KgCashPrice { get; set; }
    public decimal? KgCreditPrice { get; set; }
    public string? RawJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class KhbCategoryMap
{
    public long Id { get; set; }
    public string SourceKey { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? CategorySlug { get; set; }
    public long? WooCategoryId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class KhbCommodity
{
    public long Id { get; set; }
    public string SourceKey { get; set; } = string.Empty;
    public string? CommodityName { get; set; }
    public string? CommoditySlug { get; set; }
    public long? WooCommodityId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class KhbPackageType
{
    public long Id { get; set; }
    public string SourceKey { get; set; } = string.Empty;
    public string? PackageGroup { get; set; }
    public string? PackageCode { get; set; }
    public string? PackageTitle { get; set; }
    public decimal? UnitWeightKg { get; set; }
    public int? PacksPerCarton { get; set; }
    public decimal? PackagingPricePerPack { get; set; }
    public long? WooPackageId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class KhbProductFinal
{
    public long Id { get; set; }
    public string SourceKey { get; set; } = string.Empty;
    public long? MainGroupId { get; set; }
    public string? CategorySourceKey { get; set; }
    public string? CommoditySourceKey { get; set; }
    public string? PackageSourceKey { get; set; }
    public string? ProductName { get; set; }
    public string? ProductEnglishName { get; set; }
    public string? ProductSlug { get; set; }
    public long? WooProductId { get; set; }
    public string? Sku { get; set; }
    public string? PackageGroup { get; set; }
    public string? PackageCode { get; set; }
    public decimal? UnitWeightKg { get; set; }
    public int? PacksPerCarton { get; set; }
    public decimal? PackagingPricePerPack { get; set; }
    public decimal? KgCashPrice { get; set; }
    public decimal? KgCreditPrice { get; set; }
    public decimal? SaleCashPrice { get; set; }
    public decimal? SaleCreditPrice { get; set; }
    public decimal? BuyCashPrice { get; set; }
    public decimal? BuyCreditPrice { get; set; }
    public string? Status { get; set; }
    public string? CatalogVisibility { get; set; }
    public string? WooPayloadJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string? BrandName { get; set; }
    public string? BrandEnglishName { get; set; }
    public string? PackageTitle { get; set; }
    public decimal? BulkWeightKg { get; set; }
    public decimal? MinPurchaseKg { get; set; }
    public string? ImageTag { get; set; }
    public string? SaleMode { get; set; }
    public string? PriceCalculationBasis { get; set; }
}

public sealed class KhbProductUpdateQueue
{
    public long Id { get; set; }
    public string SourceKey { get; set; } = string.Empty;
    public string EntityType { get; set; } = "product";
    public string QueueStatus { get; set; } = "pending";
    public string ActionType { get; set; } = "upsert";
    public string? Sku { get; set; }
    public string? ProductSlug { get; set; }
    public long? WooProductId { get; set; }
    public string? WooPayloadJson { get; set; }
    public string? LastError { get; set; }
    public Guid? JobId { get; set; }
    public int TryCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class KhbProductPriceHistory
{
    public long Id { get; set; }
    public string ProductSourceKey { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public string? Sku { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public string? PackageGroup { get; set; }
    public string? PackageCode { get; set; }
    public string PriceType { get; set; } = string.Empty;
    public decimal PriceAmount { get; set; }
    public string CurrencyCode { get; set; } = "TOMAN";
    public DateTime ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
    public bool IsCurrent { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class KhbProductChangeLog
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Payload { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class KhbImportedWooCommerceRecord
{
    public long Id { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public string? Slug { get; set; }
    public string? Title { get; set; }
    public string RawJson { get; set; } = string.Empty;
    public DateTime ImportedAtUtc { get; set; }
    public string? SourceUrl { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class KhbWorkflowJob
{
    public long Id { get; set; }
    public Guid JobId { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string CurrentStep { get; set; } = "Pending";
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public int DraftCount { get; set; }
    public int SkippedCount { get; set; }
    public int PendingCount { get; set; }
    public int ProgressPercent { get; set; }
    public string? Message { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public ICollection<KhbWorkflowJobLog> Logs { get; set; } = new List<KhbWorkflowJobLog>();
}

public sealed class KhbWorkflowJobLog
{
    public long Id { get; set; }
    public long WorkflowJobId { get; set; }
    public Guid JobId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Sku { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? RequestUrl { get; set; }
    public int? ResponseCode { get; set; }
    public string? ResponseBodySummary { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public KhbWorkflowJob Job { get; set; } = null!;
}

public sealed class WooCommerceConnectionProfile
{
    public int Id { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public string EnvironmentType { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ConsumerKey { get; set; } = string.Empty;
    public string ProtectedConsumerSecret { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "wc/v3";
    public bool VerifySsl { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    public bool IsActive { get; set; }
    public DateTime? LastTestedAtUtc { get; set; }
    public bool? LastTestSucceeded { get; set; }
    public string? LastTestMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
