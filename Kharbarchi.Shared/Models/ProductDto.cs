using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kharbarchi.Shared.Models;

// در فایل Shared/DTOs/ProductDto.cs
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Description { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }

    // این لیست بسیار مهم است
    public List<ProductVariantDto> Variants { get; set; } = new();

    // این‌ها می‌توانند قیمت پیش‌فرض (ارزان‌ترین) باشند
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
}

public class ProductVariantDto
{
    public int Id { get; set; } // VariantId
    public string Name { get; set; } = string.Empty; // مثلا: 10 کیلوگرم
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsDefault {  get; set; }
}
