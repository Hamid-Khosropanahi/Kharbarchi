namespace Kharbarchi.Shared.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }

}