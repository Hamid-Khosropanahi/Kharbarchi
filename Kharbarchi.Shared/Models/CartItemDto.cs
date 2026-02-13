using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kharbarchi.Shared.Models;

public class CartItemDto
{
    public int ProductId { get; set; }
    public int VariantId { get; set; } // **جدید: شناسه تنوع**
    public string ProductName { get; set; }
    public string VariantName { get; set; } // **جدید: نام تنوع (مثلا 10 کیلو)**
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public string? ImageUrl { get; set; }


    public decimal LineTotal => UnitPrice * Quantity;
}