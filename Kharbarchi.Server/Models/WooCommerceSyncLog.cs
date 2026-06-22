namespace Kharbarchi.Server.Models;

public sealed class WooCommerceSyncLog
{
    public long Id { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string Operation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? LocalEntityId { get; set; }
    public long? WooCommerceEntityId { get; set; }
    public string? RequestHash { get; set; }
    public string? Message { get; set; }
    public string? PerformedByUserName { get; set; }
}
