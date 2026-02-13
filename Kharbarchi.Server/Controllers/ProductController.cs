using Kharbarchi.Server.Data;
using Kharbarchi.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]

public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    // ... imports

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants) // حتما باید اینکلود شود
            .ToListAsync();

        var result = products.Select(p => MapToDto(p)).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var p = await _context.Products
            .Include(x => x.Category)
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return NotFound();

        return Ok(MapToDto(p));
    }

    // یک متد کمکی برای تبدیل به DTO که کد تکراری نشود
    private static ProductDto MapToDto(Product p)
    {
        // پیدا کردن قیمت پیش‌فرض یا کمترین قیمت
        var defaultVariant = p.Variants.FirstOrDefault(v => v.IsDefault)
                             ?? p.Variants.OrderBy(v => v.Price).FirstOrDefault();

        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Description = p.Description,
            IsAvailable = p.IsAvailable,
            CategoryName = p.Category?.Name ?? "",
            ImageUrl = p.ImageUrl,

            // محاسبه مقادیر از روی Variantها
            Price = defaultVariant?.Price ?? 0,
            OldPrice = defaultVariant?.OldPrice,
            StockQuantity = p.Variants.Sum(v => v.StockQuantity),

            Variants = p.Variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Name = v.Name,
                Price = v.Price,
                OldPrice = v.OldPrice,
                StockQuantity = v.StockQuantity,
                IsDefault = v.IsDefault
            }).ToList()
        };
    }

}