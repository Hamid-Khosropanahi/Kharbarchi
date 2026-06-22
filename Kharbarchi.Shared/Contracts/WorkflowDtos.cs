using System.ComponentModel.DataAnnotations;
using Kharbarchi.Shared.Models;

namespace Kharbarchi.Shared.Contracts;

public sealed record PriceProposalCreateRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    public int? ProductVariantId { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal ProposedSalePrice { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? ProposedPurchasePrice { get; init; }

    [StringLength(1000)]
    public string? Reason { get; init; }
}

public sealed record InventoryProposalCreateRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    public int? ProductVariantId { get; init; }

    public InventoryAdjustmentKind AdjustmentKind { get; init; } = InventoryAdjustmentKind.SetAbsoluteStock;

    [Range(0, int.MaxValue)]
    public int ProposedQuantity { get; init; }

    [StringLength(1000)]
    public string? Reason { get; init; }
}

public sealed record ApprovalRequest
{
    [StringLength(1000)]
    public string? Note { get; init; }
}

public sealed record RejectRequest
{
    [Required, StringLength(1000, MinimumLength = 2)]
    public string Reason { get; init; } = string.Empty;
}

public sealed record PriceProposalDto
{
    public long Id { get; init; }
    public int ProductId { get; init; }
    public int? ProductVariantId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? VariantName { get; init; }
    public string? Sku { get; init; }
    public decimal CurrentSalePrice { get; init; }
    public decimal ProposedSalePrice { get; init; }
    public decimal? CurrentPurchasePrice { get; init; }
    public decimal? ProposedPurchasePrice { get; init; }
    public WorkflowStatus Status { get; init; }
    public string CreatedByUserName { get; init; } = string.Empty;
    public string? ManagerApprovedByUserName { get; init; }
    public string? SuperAdminApprovedByUserName { get; init; }
    public string? Reason { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed record InventoryProposalDto
{
    public long Id { get; init; }
    public int ProductId { get; init; }
    public int? ProductVariantId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? VariantName { get; init; }
    public string? Sku { get; init; }
    public int CurrentStockQuantity { get; init; }
    public int ProposedQuantity { get; init; }
    public int FinalStockQuantity { get; init; }
    public InventoryAdjustmentKind AdjustmentKind { get; init; }
    public WorkflowStatus Status { get; init; }
    public string CreatedByUserName { get; init; } = string.Empty;
    public string? ManagerApprovedByUserName { get; init; }
    public string? SuperAdminApprovedByUserName { get; init; }
    public string? Reason { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed record SyncOutboxDto(
    long Id,
    string EventType,
    string AggregateType,
    long AggregateId,
    OutboxStatus Status,
    string SourceWorkflow,
    string QueuedByUserName,
    DateTime CreatedAtUtc,
    int RetryCount,
    string? LastError);

public sealed record SyncOutboxPayloadDto(long Id, string EventType, string PayloadJson);

public sealed record MarkOutboxSentRequest
{
    [StringLength(1000)]
    public string? Note { get; init; }
}

public sealed record MarkOutboxFailedRequest
{
    [Required, StringLength(1000, MinimumLength = 2)]
    public string Error { get; init; } = string.Empty;
}
