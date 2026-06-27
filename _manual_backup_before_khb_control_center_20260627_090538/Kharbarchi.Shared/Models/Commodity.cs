using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Models;

public sealed class Commodity
{
    public int Id { get; set; }

    [Required, MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? EnglishName { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Product> Products { get; set; } = [];
}
