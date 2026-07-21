namespace Kharbarchi.Shared.Models;

public sealed class CustomerCreditHistory
{
    public long Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal PreviousCreditLimit { get; set; }
    public decimal NewCreditLimit { get; set; }
    public bool PreviousBlocked { get; set; }
    public bool NewBlocked { get; set; }
    public string Source { get; set; } = "BarokExcel";
    public string ChangedByUserName { get; set; } = string.Empty;
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}
