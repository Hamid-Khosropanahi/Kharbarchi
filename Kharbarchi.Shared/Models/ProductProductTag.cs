namespace Kharbarchi.Shared.Models;

public sealed class ProductProductTag
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int ProductTagId { get; set; }
    public ProductTag? ProductTag { get; set; }
}
