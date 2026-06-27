using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Contracts;

public sealed record KharbarchiControlSummaryDto(
    int TotalProducts,
    int GreenProducts,
    int YellowProducts,
    int RedProducts,
    int PendingWooSync,
    int FailedWooSync,
    int SyncedWooProducts,
    int LowStockProducts,
    int OpenOrders,
    int PaymentPendingOrders);

public sealed record KharbarchiProductControlListItemDto(
    int ProductId,
    string Name,
    string? Sku,
    string CategoryName,
    string? BrandName,
    string? CommodityName,
    decimal SaleCreditPrice,
    decimal? SaleCashPrice,
    decimal? CashDiscount,
    decimal? SaleCreditPricePerKg,
    decimal? CashDiscountPerKg,
    decimal? TotalWeightKg,
    int StockQuantity,
    int? WooStockQuantity,
    string PackageGroup,
    string? PackageTitle,
    string PriceCheckStatus,
    string PriceCheckCode,
    string? PriceCheckNote,
    string WooSyncStatus,
    long? WooCommerceProductId,
    bool IsAvailable);

public sealed record KharbarchiProductEditDto
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public string? Description { get; init; }
    public int CategoryId { get; init; }
    public int? BrandId { get; init; }
    public int? CommodityId { get; init; }
    public string? ImageUrl { get; init; }
    public string? GalleryJson { get; init; }
    public bool IsAvailable { get; init; }
    public int StockQuantity { get; init; }
    public int? MinStockAlertQuantity { get; init; }
    public long? WooCommerceProductId { get; init; }

    public string PriceSourceMode { get; init; } = "final_price";
    public string PackageGroup { get; init; } = "none";
    public string? PackageCode { get; init; }
    public string? PackageTitle { get; init; }
    public string? ImageTag { get; init; }
    public decimal? UnitWeightKg { get; init; }
    public int? ProductCartonCount { get; init; }
    public decimal? BulkWeightKg { get; init; }
    public decimal? MinPurchaseKg { get; init; }
    public int MinCartons { get; init; } = 1;
    public int MaxCartons { get; init; }
    public int CartonStep { get; init; } = 1;
    public string SaleUnit { get; init; } = "carton";
    public string WoodmartPriceUnitOfMeasure { get; init; } = "کارتن";

    public decimal? SaleCashPrice { get; init; }
    public decimal? SaleCreditPrice { get; init; }
    public decimal? BuyCashPrice { get; init; }
    public decimal? BuyCreditPrice { get; init; }
    public decimal? SaleCashPricePerKg { get; init; }
    public decimal? SaleCreditPricePerKg { get; init; }
    public decimal? BuyCashPricePerKg { get; init; }
    public decimal? BuyCreditPricePerKg { get; init; }

    public decimal? ExpectedSaleCreditPrice { get; init; }
    public decimal? ExpectedSaleCashPrice { get; init; }
    public decimal? ExpectedBuyCreditPrice { get; init; }
    public decimal? ExpectedBuyCashPrice { get; init; }
    public decimal? PriceCheckAmount { get; init; }
    public decimal? PriceCheckPercent { get; init; }
    public string PriceCheckStatus { get; init; } = "red";
    public string PriceCheckCode { get; init; } = "NEED_FIX";
    public string? PriceCheckNote { get; init; }
    public string WooSyncStatus { get; init; } = "pending";
    public string? WooLastError { get; init; }
}

public sealed record KharbarchiProductUpdateRequest
{
    [Required, StringLength(250, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(280)]
    public string? Slug { get; init; }

    [StringLength(120)]
    public string? Sku { get; init; }

    [StringLength(4000)]
    public string? Description { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }

    public int? BrandId { get; init; }
    public int? CommodityId { get; init; }

    [StringLength(1000)]
    public string? ImageUrl { get; init; }

    public string? GalleryJson { get; init; }
    public bool IsAvailable { get; init; } = true;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    public int? MinStockAlertQuantity { get; init; }
    public long? WooCommerceProductId { get; init; }

    [Required, StringLength(80)]
    public string PriceSourceMode { get; init; } = "final_price";

    [Required, StringLength(50)]
    public string PackageGroup { get; init; } = "none";

    [StringLength(80)]
    public string? PackageCode { get; init; }

    [StringLength(300)]
    public string? PackageTitle { get; init; }

    [StringLength(300)]
    public string? ImageTag { get; init; }

    public decimal? UnitWeightKg { get; init; }
    public int? ProductCartonCount { get; init; }
    public decimal? BulkWeightKg { get; init; }
    public decimal? MinPurchaseKg { get; init; }
    public int MinCartons { get; init; } = 1;
    public int MaxCartons { get; init; }
    public int CartonStep { get; init; } = 1;

    [StringLength(50)]
    public string SaleUnit { get; init; } = "carton";

    [StringLength(80)]
    public string WoodmartPriceUnitOfMeasure { get; init; } = "کارتن";

    public decimal? SaleCashPrice { get; init; }
    public decimal? SaleCreditPrice { get; init; }
    public decimal? BuyCashPrice { get; init; }
    public decimal? BuyCreditPrice { get; init; }
    public decimal? SaleCashPricePerKg { get; init; }
    public decimal? SaleCreditPricePerKg { get; init; }
    public decimal? BuyCashPricePerKg { get; init; }
    public decimal? BuyCreditPricePerKg { get; init; }
}

public sealed record KharbarchiInventoryUpdateRequest
{
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    public int? MinStockAlertQuantity { get; init; }
    public bool SendToWooCommerce { get; init; } = true;

    [StringLength(1000)]
    public string? Note { get; init; }
}

public sealed record KharbarchiPriceUpdateRequest
{
    public decimal? SaleCreditPrice { get; init; }
    public decimal? SaleCashPrice { get; init; }
    public decimal? BuyCreditPrice { get; init; }
    public decimal? BuyCashPrice { get; init; }
    public decimal? SaleCreditPricePerKg { get; init; }
    public decimal? SaleCashPricePerKg { get; init; }
    public decimal? BuyCreditPricePerKg { get; init; }
    public decimal? BuyCashPricePerKg { get; init; }
    public bool SendToWooCommerce { get; init; } = true;

    [StringLength(1000)]
    public string? Note { get; init; }
}

public sealed record KharbarchiWooSyncItemResultDto(
    int ProductId,
    string Name,
    string? Sku,
    long? WooCommerceProductId,
    string Status,
    string Message,
    string? PriceCheckStatus);

public sealed record KharbarchiWooSyncResultDto(
    int TotalCandidates,
    int Synced,
    int Skipped,
    int Failed,
    int Pending,
    int Percent,
    IReadOnlyList<KharbarchiWooSyncItemResultDto> Items);

public sealed record KharbarchiOrderControlListItemDto(
    long LocalOrderId,
    long WooCommerceOrderId,
    string WooCommerceOrderNumber,
    string WooCommerceStatus,
    string InternalStatus,
    string PaymentStatus,
    string CustomerFullName,
    string? CustomerPhone,
    decimal TotalAmount,
    decimal ReceivedAmount,
    string? Currency,
    DateTime? WooCreatedAtUtc);

public sealed record KharbarchiOrderConfirmRequest
{
    [Required, StringLength(80)]
    public string InternalStatus { get; init; } = "ReadyToShip";

    public bool SendToWooCommerce { get; init; } = true;

    [StringLength(1000)]
    public string? Note { get; init; }
}

public sealed record KharbarchiOrderReceivedPaymentRequest
{
    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal ReceivedAmount { get; init; }

    [StringLength(80)]
    public string PaymentType { get; init; } = "credit";

    public bool SetPaidWhenFull { get; init; } = true;
    public bool SendToWooCommerce { get; init; } = true;

    [StringLength(1000)]
    public string? Note { get; init; }
}
