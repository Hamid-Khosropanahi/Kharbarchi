namespace Kharbarchi.Shared.Contracts.ProductWorkflow;

public sealed class WorkflowJobStartRequest
{
    public string Type { get; set; } = string.Empty;
}

public sealed class WorkflowJobDto
{
    public Guid JobId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public int DraftCount { get; set; }
    public int SkippedCount { get; set; }
    public int PendingCount { get; set; }
    public int ProgressPercent { get; set; }
    public string? Message { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class WorkflowJobLogDto
{
    public long Id { get; set; }
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
}
