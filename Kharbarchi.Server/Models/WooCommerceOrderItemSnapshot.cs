namespace Kharbarchi.Server.Models;

public sealed class WooCommerceOrderItemSnapshot
{
    public long Id { get; set; }
    public long WooCommerceOrderSnapshotId { get; set; }
    public WooCommerceOrderSnapshot? Order { get; set; }
    public long WooCommerceLineItemId { get; set; }
    public long? WooCommerceProductId { get; set; }
    public long? WooCommerceVariationId { get; set; }
    public string? Sku { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UnitType { get; set; } = "کارتن";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? RawJson { get; set; }
}
