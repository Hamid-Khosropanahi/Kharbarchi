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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                IsAvailable = p.IsAvailable,
                CategoryName = p.Category!.Name,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var p = await _context.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return NotFound();

        var dto = new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Description = p.Description,
            Price = p.Price,
            IsAvailable = p.IsAvailable,
            CategoryName = p.Category!.Name,
            ImageUrl = p.ImageUrl,
            StockQuantity = p.StockQuantity
        };

        return Ok(dto);
    }

    [HttpGet("by-category/{slug}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(string slug)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Category!.Slug == slug)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                IsAvailable = p.IsAvailable,
                CategoryName = p.Category!.Name,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity
            })
            .ToListAsync();

        return Ok(products);
    }
}