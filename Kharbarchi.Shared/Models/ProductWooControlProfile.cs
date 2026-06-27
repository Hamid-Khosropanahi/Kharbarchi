using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kharbarchi.Shared.Models;

public sealed class ProductWooControlProfile
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [MaxLength(80)]
    public string PriceSourceMode { get; set; } = "final_price";

    [MaxLength(50)]
    public string PackageGroup { get; set; } = "none";

    [MaxLength(80)]
    public string? PackageCode { get; set; }

    [MaxLength(300)]
    public string? PackageTitle { get; set; }

    [MaxLength(300)]
    public string? ImageTag { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? UnitWeightKg { get; set; }

    public int? ProductCartonCount { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? BulkWeightKg { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? MinPurchaseKg { get; set; }

    public int MinCartons { get; set; } = 1;
    public int MaxCartons { get; set; }
    public int CartonStep { get; set; } = 1;

    [MaxLength(50)]
    public string SaleUnit { get; set; } = "carton";

    [MaxLength(80)]
    public string WoodmartPriceUnitOfMeasure { get; set; } = "کارتن";

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SaleCashPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SaleCreditPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BuyCashPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BuyCreditPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SaleCashPricePerKg { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SaleCreditPricePerKg { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BuyCashPricePerKg { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BuyCreditPricePerKg { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ExpectedSaleCreditPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ExpectedSaleCashPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ExpectedBuyCreditPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ExpectedBuyCashPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SaleCreditDiff { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SaleCashDiff { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BuyCreditDiff { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BuyCashDiff { get; set; }

    [MaxLength(20)]
    public string PriceCheckStatus { get; set; } = "red";

    [MaxLength(120)]
    public string PriceCheckCode { get; set; } = "NEED_FIX";

    [MaxLength(2000)]
    public string? PriceCheckNote { get; set; }

    [Column(TypeName = "decimal(9,4)")]
    public decimal? PriceCheckPercent { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PriceCheckAmount { get; set; }

    public bool NeedFix { get; set; }
    public bool AutoDraftRequired { get; set; }

    [MaxLength(50)]
    public string WooSyncStatus { get; set; } = "pending";

    [MaxLength(2000)]
    public string? WooLastError { get; set; }

    public DateTime? WooSyncedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
