using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Models;

public sealed class ProductTag
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(140)]
    public string Slug { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public List<ProductProductTag> ProductTags { get; set; } = [];
}
