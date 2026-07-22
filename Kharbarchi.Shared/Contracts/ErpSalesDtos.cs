using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Contracts;

public sealed record CustomerListItemDto(
    int Id,
    string FullName,
    string PhoneNumber,
    string? LegalEntityId,
    string? NationalCode,
    string CustomerType,
    string? StoreName,
    string? Province,
    string AddressLine,
    string City,
    decimal CreditLimit,
    decimal UsedCredit,
    decimal AvailableCredit,
    bool IsCreditBlocked,
    bool IsActive,
    DateTime? LastImportedAtUtc);

public sealed record PagedCustomerResultDto(
    IReadOnlyList<CustomerListItemDto> Items,
    int Page,
    int PageSize,
    long Total,
    int TotalPages);

public static class CustomerTypes
{
    public const string Individual = "Individual";
    public const string Legal = "Legal";
}

public sealed record CustomerImportResultDto(
    int CustomerRows,
    int CreditRows,
    int Inserted,
    int Updated,
    int CreditChanges,
    IReadOnlyList<string> Errors);

public sealed record ErpCatalogItemDto(
    int ProductId,
    int? ProductVariantId,
    string Name,
    string? VariantName,
    string? Sku,
    decimal Price,
    decimal? OldPrice,
    int StockQuantity,
    string? BrandName,
    string CategoryName,
    string? CommodityName,
    string? PackageTitle,
    string? ImageUrl);

public sealed record ErpOrderLineRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }
    public int? ProductVariantId { get; init; }
    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}

public sealed record ErpOrderCreateRequest
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; init; }
    [Required, StringLength(1000, MinimumLength = 3)]
    public string DeliveryAddressLine { get; init; } = string.Empty;
    [Required, StringLength(150, MinimumLength = 2)]
    public string DeliveryCity { get; init; } = string.Empty;
    [StringLength(30)]
    public string? DeliveryPostalCode { get; init; }
    [StringLength(2000)]
    public string? Description { get; init; }
    [Range(typeof(decimal), "0", "999999999999")]
    public decimal TotalDiscount { get; init; }
    [Required, StringLength(50)]
    public string PaymentMethod { get; init; } = "Credit";
    [MinLength(1)]
    public IReadOnlyList<ErpOrderLineRequest> Items { get; init; } = [];
}

public sealed record ErpOrderCreatedDto(int OrderId, decimal GrossAmount, decimal TotalDiscount, decimal TotalAmount);
