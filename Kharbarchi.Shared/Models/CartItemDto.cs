namespace Kharbarchi.Shared.Models;

public class CartItemDto
{
    public int ProductId { get; set; }

    // اگر محصول Variant ندارد، مقدار 0 بفرست. قیمت خود محصول استفاده می‌شود.
    public int VariantId { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}
