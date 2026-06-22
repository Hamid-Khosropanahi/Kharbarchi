using System.ComponentModel.DataAnnotations;
using Kharbarchi.Shared.Models;

namespace Kharbarchi.Shared.Contracts;

public sealed record LookupItemDto(int Id, string Name, string Slug);

public sealed record CatalogLookupsDto(
    IReadOnlyList<LookupItemDto> Categories,
    IReadOnlyList<LookupItemDto> Brands,
    IReadOnlyList<LookupItemDto> Commodities,
    IReadOnlyList<LookupItemDto> Tags,
    IReadOnlyList<SpecDefinitionDto> SpecDefinitions);

public sealed record SpecDefinitionDto(int Id, string Name, string Slug, string? Unit, int SortOrder);

public sealed record BrandUpsertRequest
{
    [Required, StringLength(160, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(180)]
    public string? Slug { get; init; }

    [StringLength(1000)]
    public string? LogoUrl { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed record CommodityUpsertRequest
{
    [Required, StringLength(180, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(200)]
    public string? Slug { get; init; }

    [StringLength(120)]
    public string? EnglishName { get; init; }

    [StringLength(1000)]
    public string? Description { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed record ProductTagUpsertRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(140)]
    public string? Slug { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed record SpecDefinitionUpsertRequest
{
    [Required, StringLength(140, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(160)]
    public string? Slug { get; init; }

    [StringLength(40)]
    public string? Unit { get; init; }

    public int SortOrder { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed record ProductUpsertRequest
{
    [Required, StringLength(250, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(280)]
    public string? Slug { get; init; }

    [StringLength(120)]
    public string? Sku { get; init; }

    [StringLength(4000)]
    public string? Description { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }

    public int? BrandId { get; init; }
    public int? CommodityId { get; init; }

    [StringLength(1000)]
    public string? ImageUrl { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal Price { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? OldPrice { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? PurchasePrice { get; init; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    public int? MinStockAlertQuantity { get; init; }
    public bool IsAvailable { get; init; } = true;
    public long? WooCommerceProductId { get; init; }

    public IReadOnlyList<int> TagIds { get; init; } = [];
    public IReadOnlyList<ProductSpecValueUpsertDto> Specs { get; init; } = [];
    public IReadOnlyList<ProductVariantUpsertDto> Variants { get; init; } = [];
}

public sealed record ProductVariantUpsertDto
{
    public int? Id { get; init; }

    [Required, StringLength(150, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [StringLength(120)]
    public string? Sku { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal Price { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? OldPrice { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? PurchasePrice { get; init; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    public int? MinStockAlertQuantity { get; init; }
    public bool IsDefault { get; init; }
    public bool IsAvailable { get; init; } = true;
    public long? WooCommerceProductId { get; init; }
    public long? WooCommerceVariationId { get; init; }
}

public sealed record ProductSpecValueUpsertDto
{
    [Range(1, int.MaxValue)]
    public int SpecDefinitionId { get; init; }

    [Required, StringLength(500, MinimumLength = 1)]
    public string Value { get; init; } = string.Empty;
}

public sealed record AdminProductListItemDto(
    int Id,
    string Name,
    string? Sku,
    string CategoryName,
    string? BrandName,
    string? CommodityName,
    decimal? Price,
    decimal? PurchasePrice,
    int StockQuantity,
    bool IsAvailable,
    long? WooCommerceProductId);

public sealed record AdminProductDetailsDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public string Description { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public int? BrandId { get; init; }
    public int? CommodityId { get; init; }
    public string? ImageUrl { get; init; }
    public decimal Price { get; init; }
    public decimal? OldPrice { get; init; }
    public decimal? PurchasePrice { get; init; }
    public int StockQuantity { get; init; }
    public int? MinStockAlertQuantity { get; init; }
    public bool IsAvailable { get; init; }
    public long? WooCommerceProductId { get; init; }
    public IReadOnlyList<int> TagIds { get; init; } = [];
    public IReadOnlyList<ProductSpecValueUpsertDto> Specs { get; init; } = [];
    public IReadOnlyList<ProductVariantUpsertDto> Variants { get; init; } = [];
}
