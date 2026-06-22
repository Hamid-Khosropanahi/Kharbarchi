namespace Kharbarchi.Shared.Models;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Description { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public string? BrandName { get; set; }
    public string? CommodityName { get; set; }
    public bool IsAvailable { get; set; }
    public string? Sku { get; set; }
    public long? WooCommerceProductId { get; set; }
    public int StockQuantity { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<ProductSpecDto> Specs { get; set; } = [];
    public List<ProductVariantDto> Variants { get; set; } = [];
}

public class ProductVariantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsDefault { get; set; }
    public bool IsAvailable { get; set; }
    public long? WooCommerceProductId { get; set; }
    public long? WooCommerceVariationId { get; set; }
}

public sealed class ProductSpecDto
{
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string Value { get; set; } = string.Empty;
}
