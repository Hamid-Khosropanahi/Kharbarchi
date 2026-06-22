using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Models;

public sealed class InventoryProposal
{
    public long Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public int CurrentStockQuantity { get; set; }
    public int ProposedQuantity { get; set; }
    public int FinalStockQuantity { get; set; }
    public InventoryAdjustmentKind AdjustmentKind { get; set; }
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
