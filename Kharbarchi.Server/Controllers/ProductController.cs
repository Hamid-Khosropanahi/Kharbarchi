using Kharbarchi.Server.Data;
using Kharbarchi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ProductController : ControllerBase
{
    private const int MaxPageSize = 50;
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Commodity)
            .Include(p => p.Variants)
            .Include(p => p.ProductTags).ThenInclude(x => x.ProductTag)
            .Include(p => p.SpecValues).ThenInclude(x => x.SpecDefinition)
            .Where(p => p.IsAvailable)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                p.Slug.Contains(term) ||
                (p.Sku != null && p.Sku.Contains(term)));
        }

        var products = await query
            .OrderBy(p => p.Category!.Name)
            .ThenBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(products.Select(MapToDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Commodity)
            .Include(x => x.Variants)
            .Include(x => x.ProductTags).ThenInclude(x => x.ProductTag)
            .Include(x => x.SpecValues).ThenInclude(x => x.SpecDefinition)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsAvailable, cancellationToken);

        return product is null ? NotFound() : Ok(MapToDto(product));
    }

    [HttpGet("slug/{slug}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<ProductDto>> GetBySlug(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest("Slug is required.");
        }

        var product = await _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Commodity)
            .Include(x => x.Variants)
            .Include(x => x.ProductTags).ThenInclude(x => x.ProductTag)
            .Include(x => x.SpecValues).ThenInclude(x => x.SpecDefinition)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.IsAvailable, cancellationToken);

        return product is null ? NotFound() : Ok(MapToDto(product));
    }

    private static ProductDto MapToDto(Product product)
    {
        var defaultVariant = product.Variants
            .Where(v => v.IsAvailable && v.StockQuantity > 0)
            .OrderByDescending(v => v.IsDefault)
            .ThenBy(v => v.Price)
            .FirstOrDefault()
            ?? product.Variants.Where(v => v.IsAvailable).OrderBy(v => v.Price).FirstOrDefault();

        var price = defaultVariant?.Price ?? product.Price;
        var oldPrice = defaultVariant?.OldPrice ?? product.OldPrice;
        var stockQuantity = product.Variants.Count == 0
            ? product.StockQuantity
            : product.Variants.Where(x => x.IsAvailable).Sum(v => v.StockQuantity);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            IsAvailable = product.IsAvailable,
            CategoryName = product.Category?.Name ?? string.Empty,
            BrandName = product.Brand?.Name,
            CommodityName = product.Commodity?.Name,
            ImageUrl = product.ImageUrl,
            Sku = product.Sku,
            WooCommerceProductId = product.WooCommerceProductId,
            Price = price,
            OldPrice = oldPrice,
            StockQuantity = stockQuantity,
            Tags = product.ProductTags.Select(x => x.ProductTag!.Name).OrderBy(x => x).ToList(),
            Specs = product.SpecValues
                .OrderBy(x => x.SpecDefinition!.SortOrder)
                .ThenBy(x => x.SpecDefinition!.Name)
                .Select(x => new ProductSpecDto
                {
                    Name = x.SpecDefinition!.Name,
                    Unit = x.SpecDefinition.Unit,
                    Value = x.Value
                })
                .ToList(),
            Variants = product.Variants
                .Where(x => x.IsAvailable)
                .OrderByDescending(v => v.IsDefault)
                .ThenBy(v => v.Price)
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Sku = v.Sku,
                    Price = v.Price,
                    OldPrice = v.OldPrice,
                    StockQuantity = v.StockQuantity,
                    IsDefault = v.IsDefault,
                    IsAvailable = v.IsAvailable,
                    WooCommerceProductId = v.WooCommerceProductId,
                    WooCommerceVariationId = v.WooCommerceVariationId
                })
                .ToList()
        };
    }
}
