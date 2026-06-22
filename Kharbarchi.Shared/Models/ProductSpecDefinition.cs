using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Models;

public sealed class ProductSpecDefinition
{
    public int Id { get; set; }

    [Required, MaxLength(140)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(160)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Unit { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public List<ProductSpecValue> Values { get; set; } = [];
}
