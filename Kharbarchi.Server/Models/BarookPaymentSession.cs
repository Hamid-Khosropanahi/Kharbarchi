namespace Kharbarchi.Server.Models;

public sealed class BarookPaymentSession
{
    public long Id { get; set; }
    public long WooCommerceOrderSnapshotId { get; set; }
    public WooCommerceOrderSnapshot? Order { get; set; }
    public long WooCommerceOrderId { get; set; }
    public string ExternalCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRR";
    public string? Token { get; set; }
    public DateTime? TokenExpireDateUtc { get; set; }
    public string? PaymentUrl { get; set; }
    public DateTime? LinkSentAtUtc { get; set; }
    public string? BarookStatus { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? MaskedCardNumber { get; set; }
    public string? TransactionId { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAtUtc { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public string? VerifiedByUserName { get; set; }
    public string? StartRequestJson { get; set; }
    public string? StartResponseJson { get; set; }
    public string? VerifyResponseJson { get; set; }
    public string? LastError { get; set; }
    public bool IsCompleted { get; set; }
}
