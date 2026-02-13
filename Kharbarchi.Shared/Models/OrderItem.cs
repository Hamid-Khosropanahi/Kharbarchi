using System.ComponentModel.DataAnnotations.Schema;

namespace Kharbarchi.Shared.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    // --- فیلد جدید ---
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public string? VariantName { get; set; } // ذخیره نام وزن (مثلا 10 کیلو)

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; } // قیمت نهایی در لحظه خرید
    public decimal LineTotal => UnitPrice * Quantity;

}
