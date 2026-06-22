namespace Kharbarchi.Server.Models;

public sealed class ManualPaymentReceipt
{
    public long Id { get; set; }
    public long WooCommerceOrderSnapshotId { get; set; }
    public WooCommerceOrderSnapshot? Order { get; set; }
    public long WooCommerceOrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRR";
    public string ReceiptNumber { get; set; } = string.Empty;
    public string PaymentSource { get; set; } = string.Empty; // BankTransfer, Cash, POS, CardToCard, Other
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
    public string RegisteredByUserName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public bool SentToWooCommerce { get; set; }
    public DateTime? SentToWooCommerceAtUtc { get; set; }
    public string? WooCommerceResponseSummary { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
