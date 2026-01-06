using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kharbarchi.Shared.Models;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string CategoryName { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
}