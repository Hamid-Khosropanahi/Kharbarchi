using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kharbarchi.Shared.Models;

public sealed class PriceProposal
{
    public long Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentSalePrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ProposedSalePrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CurrentPurchasePrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ProposedPurchasePrice { get; set; }

    public WorkflowStatus Status { get; set; } = WorkflowStatus.Submitted;

    [Required, MaxLength(256)]
    public string CreatedByUserName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? ManagerApprovedByUserName { get; set; }

    [MaxLength(256)]
    public string? SuperAdminApprovedByUserName { get; set; }

    [MaxLength(256)]
    public string? RejectedByUserName { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    [MaxLength(1000)]
    public string? ManagerNote { get; set; }

    [MaxLength(1000)]
    public string? SuperAdminNote { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ManagerApprovedAtUtc { get; set; }
    public DateTime? SuperAdminApprovedAtUtc { get; set; }
    public DateTime? RejectedAtUtc { get; set; }
    public DateTime? QueuedForSyncAtUtc { get; set; }
    public DateTime? SyncedAtUtc { get; set; }
}
