namespace Kharbarchi.Shared.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "New"; // New, Processing, Shipped, Completed, Canceled
    public string PaymentMethod { get; set; } = "Offline"; // Offline, Online, WooCommerce
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Canceled
    public string? PaymentReference { get; set; }
    public string? GatewayName { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public long? WooCommerceOrderId { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
