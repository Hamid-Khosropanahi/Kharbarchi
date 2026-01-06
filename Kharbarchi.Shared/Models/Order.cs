namespace Kharbarchi.Shared.Models;

public class Order
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "New";        // New, Processing, Shipped, Completed
    public string PaymentMethod { get; set; } = "Offline"; // Offline, Online
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}