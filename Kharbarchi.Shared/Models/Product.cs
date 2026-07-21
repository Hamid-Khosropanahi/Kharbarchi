namespace Kharbarchi.Shared.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsAvailable { get; set; } = true;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    public int? CommodityId { get; set; }
    public Commodity? Commodity { get; set; }

    public string? ImageUrl { get; set; }
    public string? GalleryJson { get; set; }

    // قیمت فروش مستقل هر محصول. از این نسخه هیچ فرمول کیلویی برای قیمت فروش استفاده نمی‌شود.
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }

    // قیمت خرید داخلی است و هرگز نباید برای کارمند انبار یا API عمومی ارسال شود.
    public decimal? PurchasePrice { get; set; }

    public int StockQuantity { get; set; }
    public int? MinStockAlertQuantity { get; set; }

    public long? WooCommerceProductId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public List<ProductVariant> Variants { get; set; } = [];
    public List<ProductProductTag> ProductTags { get; set; } = [];
    public List<ProductSpecValue> SpecValues { get; set; } = [];
    public ProductWooControlProfile? WooControlProfile { get; set; }
    public List<ProductPriceHistory> PriceHistory { get; set; } = [];
}
