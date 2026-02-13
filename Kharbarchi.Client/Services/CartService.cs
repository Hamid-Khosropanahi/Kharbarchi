using Kharbarchi.Shared.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Kharbarchi.Client.Services
{
    public class CartService
    {
        public event Action? OnChange;

        public List<CartItemDto> Items { get; } = new();

        public void AddItem(CartItemDto item)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existing != null)
            {
                existing.Quantity += item.Quantity; // معمولا item.Quantity اینجا 1 است
            }
            else
            {
                Items.Add(item);
            }

            NotifyStateChanged();
        }

        // --- اصلاح شده: متد کاهش تعداد ---
        public void RemoveItem(int productId)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                if (existing.Quantity > 1)
                {
                    // اگر بیشتر از یکی بود، فقط تعداد را کم کن
                    existing.Quantity--;
                }
                else
                {
                    // اگر آخرین عدد بود، آیتم را از لیست حذف کن
                    Items.Remove(existing);
                }

                NotifyStateChanged();
            }
        }
        // --------------------------------

        public void Clear()
        {
            Items.Clear();
            NotifyStateChanged();
        }

        // متد کمکی برای دریافت تعداد یک محصول خاص (برای نمایش در کارت محصول)
        public int GetQuantity(int productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            return item == null ? 0 : item.Quantity;
        }

        public decimal GetTotal() => Items.Sum(i => i.UnitPrice * i.Quantity);

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
