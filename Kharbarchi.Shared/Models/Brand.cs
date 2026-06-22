using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Models;

public sealed class Brand
{
    public int Id { get; set; }

    [Required, MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(180)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public long? WooCommerceBrandId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Product> Products { get; set; } = [];
}
