using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Models;

public sealed class ProductSpecValue
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int SpecDefinitionId { get; set; }
    public ProductSpecDefinition? SpecDefinition { get; set; }

    [Required, MaxLength(500)]
    public string Value { get; set; } = string.Empty;
}
