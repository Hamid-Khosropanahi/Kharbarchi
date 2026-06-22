namespace Kharbarchi.Server.Models;

public sealed class GatewayPaymentReceipt
{
    public long Id { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAtUtc { get; set; }
    public long WooCommerceOrderId { get; set; }
    public int? LocalOrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRR";
    public string GatewayName { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? GatewayRawStatus { get; set; }
    public string? Note { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public bool SentToWooCommerce { get; set; }
    public DateTime? SentToWooCommerceAtUtc { get; set; }
    public string? WooCommerceResponseSummary { get; set; }
}
