using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Models;

public sealed class ApprovalAuditLog
{
    public long Id { get; set; }

    [Required, MaxLength(80)]
    public string EntityType { get; set; } = string.Empty;

    public long EntityId { get; set; }

    [Required, MaxLength(80)]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string UserName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string UserRole { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Note { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
