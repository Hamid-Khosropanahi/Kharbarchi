using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Models;

public sealed class SyncOutboxMessage
{
    public long Id { get; set; }

    [Required, MaxLength(120)]
    public string EventType { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string AggregateType { get; set; } = string.Empty;

    public long AggregateId { get; set; }

    [Required]
    public string PayloadJson { get; set; } = "{}";

    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;

    [Required, MaxLength(256)]
    public string QueuedByUserName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string SourceWorkflow { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? LastError { get; set; }

    [MaxLength(256)]
    public string? LockedBy { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LockedAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public int RetryCount { get; set; }
}
