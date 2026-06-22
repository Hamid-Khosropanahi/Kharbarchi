using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kharbarchi.Shared.Models;

public class ProductVariant
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty; // مثال: کارتن ۱۲ عددی، بسته ۹۰۰ گرمی، فله ۲۰ کیلویی

    public string? Sku { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OldPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PurchasePrice { get; set; }

    public int StockQuantity { get; set; }
    public int? MinStockAlertQuantity { get; set; }
    public bool IsDefault { get; set; }
    public bool IsAvailable { get; set; } = true;

    public long? WooCommerceProductId { get; set; }
    public long? WooCommerceVariationId { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
