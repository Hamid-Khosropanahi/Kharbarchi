using System.ComponentModel.DataAnnotations.Schema;

namespace Kharbarchi.Shared.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public string? VariantName { get; set; }
    public string? Sku { get; set; }
    public long? WooCommerceProductId { get; set; }
    public long? WooCommerceVariationId { get; set; }
    public int Quantity { get; set; }
    public string ProductName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OriginalUnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LineDiscount { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}
