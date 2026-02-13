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
            // شرط بررسی: هم محصول و هم وزن باید یکی باشند
            var existing = Items.FirstOrDefault(i => i.ProductId == item.ProductId && i.VariantId == item.VariantId);

            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
            NotifyStateChanged();
        }

        public void RemoveItem(int productId, int variantId) // ورودی تغییر کرد
        {
            // شرط بررسی: هم محصول و هم وزن
            var existing = Items.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);

            if (existing != null)
            {
                if (existing.Quantity > 1)
                {
                    existing.Quantity--;
                }
                else
                {
                    Items.Remove(existing);
                }
                NotifyStateChanged();
            }
        }

        // متد کمکی برای گرفتن تعداد یک وزن خاص از یک محصول
        public int GetQuantity(int productId, int variantId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);
            return item == null ? 0 : item.Quantity;
        }

        public decimal GetTotal() => Items.Sum(i => i.UnitPrice * i.Quantity);

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void Clear()
        {
            Items.Clear();
            NotifyStateChanged();
        }
    }
}
