using Kharbarchi.Shared.Models;

namespace Kharbarchi.Server.Models;

public sealed class WooCommerceOrderSnapshot
{
    public long Id { get; set; }
    public long WooCommerceOrderId { get; set; }
    public string WooCommerceOrderNumber { get; set; } = string.Empty;
    public string WooCommerceStatus { get; set; } = string.Empty;
    public string InternalStatus { get; set; } = WooOrderInternalStatus.Synced;
    public string PaymentStatus { get; set; } = PaymentStatusNames.Pending;
    public string? PaymentMethod { get; set; }
    public string? PaymentMethodTitle { get; set; }
    public string? TransactionId { get; set; }
    public string? Currency { get; set; } = "IRR";
    public decimal TotalAmount { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public string CustomerFullName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerNationalCode { get; set; }
    public string? BillingAddress { get; set; }
    public string? ShippingAddress { get; set; }
    public string? CustomerNote { get; set; }
    public DateTime? WooCreatedAtUtc { get; set; }
    public DateTime? WooUpdatedAtUtc { get; set; }
    public DateTime SyncedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastPaymentCheckedAtUtc { get; set; }
    public DateTime? ReadyToShipAtUtc { get; set; }
    public string? LastActionByUserName { get; set; }
    public string? LastActionNote { get; set; }
    public string? RawJson { get; set; }

    public List<WooCommerceOrderItemSnapshot> Items { get; set; } = [];
    public List<BarookPaymentSession> BarookPayments { get; set; } = [];
    public List<ManualPaymentReceipt> ManualReceipts { get; set; } = [];
}
