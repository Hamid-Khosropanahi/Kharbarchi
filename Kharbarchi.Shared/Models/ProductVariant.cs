using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kharbarchi.Shared.Models;

public class ProductVariant
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty; // مثال: "10 کیلوگرم"

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OldPrice { get; set; } // قیمت خط خورده برای این وزن خاص

    public int StockQuantity { get; set; }

    public bool IsDefault { get; set; } // برای نمایش قیمت اولیه

    // Foreign Key
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
