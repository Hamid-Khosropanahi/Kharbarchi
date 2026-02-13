using System;
using System.Collections.Generic;
using System.Text;

namespace Kharbarchi.Shared.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public string Name { get; set; } // مثلا: "10 کیلوگرم"
        public decimal Price { get; set; } // مثلا: 200
        public bool IsDefault { get; set; } // برای مشخص کردن پیش‌فرض (25 کیلو)
    }

}
