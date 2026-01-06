using Kharbarchi.Shared.Models;

namespace Kharbarchi.Client.Services;

public class CartService
{
    public List<CartItemDto> Items { get; } = new();

    public void AddItem(CartItemDto item)
    {
        var existing = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existing != null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            Items.Add(item);
        }
    }

    public void RemoveItem(int productId)
    {
        var existing = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
            Items.Remove(existing);
    }

    public void Clear() => Items.Clear();

    public decimal GetTotal() => Items.Sum(i => i.LineTotal);
}