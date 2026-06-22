using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kharbarchi.Server.Contracts;

public sealed record LocalProductSyncDto(
    int ProductId,
    int? VariantId,
    string Name,
    string? Sku,
    long? WooCommerceProductId,
    long? WooCommerceVariationId,
    decimal Price,
    decimal? OldPrice,
    int StockQuantity,
    bool IsAvailable);

public sealed record SyncProductsRequest
{
    public IReadOnlyList<int>? ProductIds { get; init; }
    public bool DryRun { get; init; }
}

public sealed record SyncProductsResult(
    int TotalCandidates,
    int UpdatedCount,
    int SkippedCount,
    int FailedCount,
    IReadOnlyList<SyncProductsItemResult> Items);

public sealed record SyncProductsItemResult(
    int ProductId,
    int? VariantId,
    long? WooCommerceProductId,
    long? WooCommerceVariationId,
    string Status,
    string Message);

public sealed record WooCommerceOrderQuery(
    string? Status,
    DateTime? AfterUtc,
    int Page = 1,
    int PageSize = 20);

public sealed record GatewayPaymentReceivedRequest
{
    [Range(1, long.MaxValue)]
    public long WooCommerceOrderId { get; init; }

    public int? LocalOrderId { get; init; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Amount { get; init; }

    [Required, StringLength(8, MinimumLength = 3)]
    public string Currency { get; init; } = "IRR";

    [Required, StringLength(80, MinimumLength = 2)]
    public string GatewayName { get; init; } = string.Empty;

    [Required, StringLength(160, MinimumLength = 3)]
    public string TransactionId { get; init; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 8)]
    public string IdempotencyKey { get; init; } = string.Empty;

    [Required, RegularExpression("^(Paid|Failed|Canceled)$")]
    public string PaymentStatus { get; init; } = "Paid";

    [StringLength(100)]
    public string? GatewayRawStatus { get; init; }

    public DateTime? PaidAtUtc { get; init; }

    [StringLength(1000)]
    public string? Note { get; init; }
}

public sealed record GatewayPaymentReceivedResponse(
    bool IsSuccess,
    bool IsDuplicate,
    long ReceiptId,
    long WooCommerceOrderId,
    string Message);

public sealed record WooCommerceProductUpdatePayload
{
    [JsonPropertyName("regular_price")]
    public string RegularPrice { get; init; } = string.Empty;

    [JsonPropertyName("sale_price")]
    public string? SalePrice { get; init; }

    [JsonPropertyName("manage_stock")]
    public bool ManageStock { get; init; } = true;

    [JsonPropertyName("stock_quantity")]
    public int StockQuantity { get; init; }

    [JsonPropertyName("stock_status")]
    public string StockStatus { get; init; } = "instock";

    [JsonPropertyName("status")]
    public string Status { get; init; } = "publish";
}

public sealed record WooCommercePaymentUpdatePayload
{
    [JsonPropertyName("set_paid")]
    public bool SetPaid { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "processing";

    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; init; } = string.Empty;

    [JsonPropertyName("meta_data")]
    public IReadOnlyList<WooCommerceMetaData> MetaData { get; init; } = [];
}

public sealed record WooCommerceMetaData(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] string Value);

public sealed record WooCommerceOrderNotePayload(
    [property: JsonPropertyName("note")] string Note,
    [property: JsonPropertyName("customer_note")] bool CustomerNote = false,
    [property: JsonPropertyName("added_by_user")] bool AddedByUser = true);
