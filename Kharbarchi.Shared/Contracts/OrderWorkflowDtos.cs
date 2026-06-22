using System.ComponentModel.DataAnnotations;

namespace Kharbarchi.Shared.Contracts;

public sealed record ImportWooCommerceProductsRequest
{
    public int PageSize { get; init; } = 100;
    public int MaxPages { get; init; } = 20;
    public bool DryRun { get; init; }
}

public sealed record ImportWooCommerceOrdersRequest
{
    public string? Status { get; init; }
    public DateTime? AfterUtc { get; init; }
    public int PageSize { get; init; } = 100;
    public int MaxPages { get; init; } = 20;
    public bool DryRun { get; init; }
}

public sealed record ImportResult(int Scanned, int Created, int Updated, int Skipped, IReadOnlyList<string> Messages);

public sealed record LocalWooOrderListItemDto(
    long Id,
    long WooCommerceOrderId,
    string WooCommerceOrderNumber,
    string WooCommerceStatus,
    string InternalStatus,
    string PaymentStatus,
    string CustomerFullName,
    string? CustomerPhone,
    decimal TotalAmount,
    string? Currency,
    DateTime? WooCreatedAtUtc,
    DateTime SyncedAtUtc,
    int ItemsCount);

public sealed record LocalWooOrderDetailsDto(
    long Id,
    long WooCommerceOrderId,
    string WooCommerceOrderNumber,
    string WooCommerceStatus,
    string InternalStatus,
    string PaymentStatus,
    string CustomerFullName,
    string? CustomerPhone,
    string? CustomerNationalCode,
    string? BillingAddress,
    string? ShippingAddress,
    string? CustomerNote,
    decimal TotalAmount,
    string? Currency,
    IReadOnlyList<LocalWooOrderItemDto> Items,
    IReadOnlyList<BarookPaymentSessionDto> BarookPayments,
    IReadOnlyList<ManualPaymentReceiptDto> ManualReceipts);

public sealed record LocalWooOrderItemDto(
    long Id,
    long WooCommerceLineItemId,
    long? WooCommerceProductId,
    long? WooCommerceVariationId,
    string? Sku,
    string Name,
    decimal Quantity,
    string UnitType,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record ChangeInternalOrderStatusRequest
{
    [Required, StringLength(80)]
    public string InternalStatus { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Note { get; init; }

    public bool UpdateWooCommerceNote { get; init; } = true;
}

public sealed record StartBarookPaymentRequest
{
    [Range(1, 180)]
    public int? PaymentDayCount { get; init; }

    [Range(1, 12)]
    public int? PaymentMonthCount { get; init; }

    [Required, StringLength(200)]
    public string OwnerName { get; init; } = string.Empty;

    [StringLength(20, MinimumLength = 10)]
    public string? OwnerMobile { get; init; }

    [Required, RegularExpression("^[0-9]{10}$")]
    public string OwnerNationalCode { get; init; } = string.Empty;

    [Required, Url]
    public string RedirectUrl { get; init; } = string.Empty;

    [StringLength(80)]
    public string? BranchCode { get; init; }

    [StringLength(80)]
    public string? BusinessServiceSlug { get; init; }

    [StringLength(1000)]
    public string? Note { get; init; }
}

public sealed record StartBarookPaymentResponse(
    long SessionId,
    string ExternalCode,
    string? Token,
    DateTime? ExpireDateUtc,
    string? PaymentUrl,
    string Message);

public sealed record MarkBarookPaymentLinkSentRequest
{
    [StringLength(1000)]
    public string? Note { get; init; }
}

public sealed record VerifyBarookPaymentRequest
{
    [StringLength(80)]
    public string? Token { get; init; }

    public bool SendResultToWooCommerce { get; init; } = true;
}

public sealed record VerifyBarookPaymentResponse(
    long SessionId,
    string ExternalCode,
    string? BarookStatus,
    bool IsPaid,
    string? TransactionId,
    string Message);

public sealed record BarookPaymentSessionDto(
    long Id,
    string ExternalCode,
    decimal Amount,
    string? Token,
    string? PaymentUrl,
    DateTime? LinkSentAtUtc,
    string? BarookStatus,
    string? ReferenceNumber,
    string? MaskedCardNumber,
    string? TransactionId,
    DateTime? PaidAtUtc,
    bool IsCompleted,
    DateTime CreatedAtUtc);

public sealed record CreateManualPaymentReceiptRequest
{
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Amount { get; init; }

    [Required, StringLength(8, MinimumLength = 3)]
    public string Currency { get; init; } = "IRR";

    [Required, StringLength(160, MinimumLength = 2)]
    public string ReceiptNumber { get; init; } = string.Empty;

    [Required, StringLength(80)]
    public string PaymentSource { get; init; } = "BankTransfer";

    public DateTime? PaidAtUtc { get; init; }

    [StringLength(1000)]
    public string? Note { get; init; }

    public bool SendToWooCommerce { get; init; } = true;
}

public sealed record ManualPaymentReceiptDto(
    long Id,
    decimal Amount,
    string Currency,
    string ReceiptNumber,
    string PaymentSource,
    DateTime PaidAtUtc,
    string RegisteredByUserName,
    string? Note,
    bool SentToWooCommerce,
    DateTime CreatedAtUtc);
