namespace Kharbarchi.Shared.Models;

public sealed class ProductPriceHistory
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public string PriceType { get; set; } = "Sale";
    public decimal Amount { get; set; }
    public bool IsCurrent { get; set; } = true;
    public DateTime ValidFromUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ValidToUtc { get; set; }
    public string Source { get; set; } = "ERP";
    public string ChangedByUserName { get; set; } = string.Empty;
}
