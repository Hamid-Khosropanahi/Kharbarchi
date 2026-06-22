using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Kharbarchi.Server.Contracts;
using Kharbarchi.Server.Data;
using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Services;

public sealed class WooCommerceSyncService
{
    private readonly AppDbContext _context;
    private readonly WooCommerceApiClient _wooCommerceApiClient;
    private readonly ILogger<WooCommerceSyncService> _logger;

    public WooCommerceSyncService(
        AppDbContext context,
        WooCommerceApiClient wooCommerceApiClient,
        ILogger<WooCommerceSyncService> logger)
    {
        _context = context;
        _wooCommerceApiClient = wooCommerceApiClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<LocalProductSyncDto>> GetLocalProductsAsync(CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Variants)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return products.SelectMany(ToSyncRows).ToList();
    }

    public async Task<SyncProductsResult> SyncProductsAsync(IReadOnlyCollection<int>? productIds, bool dryRun, string userName, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Variants)
            .AsQueryable();

        if (productIds is { Count: > 0 })
        {
            query = query.Where(p => productIds.Contains(p.Id));
        }

        var products = await query.OrderBy(p => p.Id).ToListAsync(cancellationToken);
        var items = new List<SyncProductsItemResult>();

        foreach (var row in products.SelectMany(ToSyncRows))
        {
            try
            {
                if (row.WooCommerceProductId is null or <= 0)
                {
                    items.Add(new SyncProductsItemResult(row.ProductId, row.VariantId, row.WooCommerceProductId, row.WooCommerceVariationId, "Skipped", "WooCommerceProductId تنظیم نشده است."));
                    continue;
                }

                var payload = BuildProductUpdatePayload(row);

                if (!dryRun)
                {
                    if (row.WooCommerceVariationId is > 0)
                    {
                        await _wooCommerceApiClient.UpdateVariationAsync(row.WooCommerceProductId.Value, row.WooCommerceVariationId.Value, payload, cancellationToken);
                    }
                    else
                    {
                        await _wooCommerceApiClient.UpdateProductAsync(row.WooCommerceProductId.Value, payload, cancellationToken);
                    }
                }

                await AddSyncLogAsync("ProductPriceStockSync", "Success", "Product", row.ProductId, row.WooCommerceProductId, dryRun ? "Dry run" : "Updated", userName, payload, cancellationToken);
                items.Add(new SyncProductsItemResult(row.ProductId, row.VariantId, row.WooCommerceProductId, row.WooCommerceVariationId, dryRun ? "DryRun" : "Updated", "قیمت و موجودی آماده/ارسال شد."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Product sync failed. ProductId={ProductId}, VariantId={VariantId}", row.ProductId, row.VariantId);
                await AddSyncLogAsync("ProductPriceStockSync", "Failed", "Product", row.ProductId, row.WooCommerceProductId, ex.Message, userName, null, cancellationToken);
                items.Add(new SyncProductsItemResult(row.ProductId, row.VariantId, row.WooCommerceProductId, row.WooCommerceVariationId, "Failed", ex.Message));
            }
        }

        return new SyncProductsResult(
            items.Count,
            items.Count(x => x.Status is "Updated" or "DryRun"),
            items.Count(x => x.Status == "Skipped"),
            items.Count(x => x.Status == "Failed"),
            items);
    }

    public Task<JsonDocument> GetWooCommerceOrdersAsync(string? status, DateTime? afterUtc, int page, int pageSize, CancellationToken cancellationToken)
    {
        return _wooCommerceApiClient.GetOrdersAsync(status, afterUtc, page, pageSize, cancellationToken);
    }

    public async Task<GatewayPaymentReceivedResponse> RegisterGatewayPaymentAsync(GatewayPaymentReceivedRequest request, string userName, CancellationToken cancellationToken)
    {
        var normalizedIdempotencyKey = request.IdempotencyKey.Trim();
        var normalizedTransactionId = request.TransactionId.Trim();

        var duplicate = await _context.GatewayPaymentReceipts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdempotencyKey == normalizedIdempotencyKey || x.TransactionId == normalizedTransactionId, cancellationToken);

        if (duplicate is not null)
        {
            return new GatewayPaymentReceivedResponse(true, true, duplicate.Id, duplicate.WooCommerceOrderId, "این پرداخت قبلاً ثبت شده بود و دوباره ارسال نشد.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var receipt = new GatewayPaymentReceipt
        {
            WooCommerceOrderId = request.WooCommerceOrderId,
            LocalOrderId = request.LocalOrderId,
            Amount = request.Amount,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            GatewayName = request.GatewayName.Trim(),
            TransactionId = normalizedTransactionId,
            IdempotencyKey = normalizedIdempotencyKey,
            PaymentStatus = request.PaymentStatus.Trim(),
            GatewayRawStatus = request.GatewayRawStatus?.Trim(),
            PaidAtUtc = request.PaidAtUtc?.ToUniversalTime() ?? DateTime.UtcNow,
            Note = request.Note?.Trim(),
            RequestedByUserName = userName,
            SentToWooCommerce = false
        };

        _context.GatewayPaymentReceipts.Add(receipt);

        if (request.LocalOrderId is > 0)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == request.LocalOrderId.Value, cancellationToken);
            if (order is not null)
            {
                order.PaymentStatus = request.PaymentStatus == "Paid" ? "Paid" : request.PaymentStatus;
                order.PaymentReference = normalizedTransactionId;
                order.GatewayName = request.GatewayName.Trim();
                order.PaidAtUtc = receipt.PaidAtUtc;
                order.WooCommerceOrderId = request.WooCommerceOrderId;
                order.Status = request.PaymentStatus == "Paid" ? "Processing" : order.Status;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (request.PaymentStatus == "Paid")
        {
            var paymentPayload = new WooCommercePaymentUpdatePayload
            {
                SetPaid = true,
                Status = "processing",
                TransactionId = normalizedTransactionId,
                MetaData =
                [
                    new WooCommerceMetaData("_khb_gateway_name", request.GatewayName.Trim()),
                    new WooCommerceMetaData("_khb_gateway_amount", request.Amount.ToString("0.##", CultureInfo.InvariantCulture)),
                    new WooCommerceMetaData("_khb_gateway_currency", request.Currency.Trim().ToUpperInvariant()),
                    new WooCommerceMetaData("_khb_idempotency_key", normalizedIdempotencyKey),
                    new WooCommerceMetaData("_khb_paid_at_utc", (receipt.PaidAtUtc ?? DateTime.UtcNow).ToString("O", CultureInfo.InvariantCulture))
                ]
            };

            using var response = await _wooCommerceApiClient.MarkOrderPaymentAsync(request.WooCommerceOrderId, paymentPayload, cancellationToken);
            receipt.SentToWooCommerce = true;
            receipt.SentToWooCommerceAtUtc = DateTime.UtcNow;
            receipt.WooCommerceResponseSummary = ExtractSummary(response.RootElement);

            var note = BuildPaymentNote(request, normalizedTransactionId);
            await _wooCommerceApiClient.AddOrderNoteAsync(request.WooCommerceOrderId, new WooCommerceOrderNotePayload(note), cancellationToken);
        }
        else
        {
            var note = $"Kharbarchi gateway callback: payment status={request.PaymentStatus}, gateway={request.GatewayName}, transaction={normalizedTransactionId}";
            await _wooCommerceApiClient.AddOrderNoteAsync(request.WooCommerceOrderId, new WooCommerceOrderNotePayload(note), cancellationToken);
            receipt.SentToWooCommerce = true;
            receipt.SentToWooCommerceAtUtc = DateTime.UtcNow;
            receipt.WooCommerceResponseSummary = "Order note added for non-paid callback.";
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new GatewayPaymentReceivedResponse(true, false, receipt.Id, request.WooCommerceOrderId, "پرداخت ثبت شد و نتیجه به WooCommerce ارسال شد.");
    }

    private static IEnumerable<LocalProductSyncDto> ToSyncRows(Product product)
    {
        if (product.Variants.Count == 0)
        {
            yield return new LocalProductSyncDto(
                product.Id,
                null,
                product.Name,
                product.Sku,
                product.WooCommerceProductId,
                null,
                product.Price,
                product.OldPrice,
                product.StockQuantity,
                product.IsAvailable);

            yield break;
        }

        foreach (var variant in product.Variants)
        {
            yield return new LocalProductSyncDto(
                product.Id,
                variant.Id,
                $"{product.Name} - {variant.Name}",
                variant.Sku ?? product.Sku,
                variant.WooCommerceProductId ?? product.WooCommerceProductId,
                variant.WooCommerceVariationId,
                variant.Price,
                variant.OldPrice,
                variant.StockQuantity,
                product.IsAvailable);
        }
    }

    private static WooCommerceProductUpdatePayload BuildProductUpdatePayload(LocalProductSyncDto row)
    {
        var regularPrice = row.OldPrice is > 0 && row.OldPrice > row.Price ? row.OldPrice.Value : row.Price;
        var salePrice = row.OldPrice is > 0 && row.OldPrice > row.Price ? row.Price : (decimal?)null;

        return new WooCommerceProductUpdatePayload
        {
            RegularPrice = regularPrice.ToString("0.##", CultureInfo.InvariantCulture),
            SalePrice = salePrice?.ToString("0.##", CultureInfo.InvariantCulture),
            StockQuantity = Math.Max(0, row.StockQuantity),
            StockStatus = row.StockQuantity > 0 ? "instock" : "outofstock",
            Status = row.IsAvailable ? "publish" : "draft"
        };
    }

    private async Task AddSyncLogAsync(string operation, string status, string entityType, int localEntityId, long? wooEntityId, string? message, string userName, object? payload, CancellationToken cancellationToken)
    {
        _context.WooCommerceSyncLogs.Add(new WooCommerceSyncLog
        {
            Operation = operation,
            Status = status,
            EntityType = entityType,
            LocalEntityId = localEntityId,
            WooCommerceEntityId = wooEntityId,
            Message = message,
            PerformedByUserName = userName,
            RequestHash = payload is null ? null : Sha256(JsonSerializer.Serialize(payload))
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string BuildPaymentNote(GatewayPaymentReceivedRequest request, string transactionId)
    {
        var note = new StringBuilder();
        note.Append("Kharbarchi payment received. ");
        note.Append($"Gateway={request.GatewayName}; ");
        note.Append($"TransactionId={transactionId}; ");
        note.Append($"Amount={request.Amount:0.##} {request.Currency}; ");
        note.Append($"PaidAtUtc={(request.PaidAtUtc?.ToUniversalTime() ?? DateTime.UtcNow):O}.");

        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            note.Append(' ');
            note.Append(request.Note.Trim());
        }

        return note.ToString();
    }

    private static string ExtractSummary(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return element.GetRawText();
        }

        var parts = new List<string>();
        if (element.TryGetProperty("id", out var id))
        {
            parts.Add($"id={id}");
        }

        if (element.TryGetProperty("status", out var status))
        {
            parts.Add($"status={status}");
        }

        if (element.TryGetProperty("transaction_id", out var tx))
        {
            parts.Add($"transaction_id={tx}");
        }

        return parts.Count == 0 ? element.GetRawText() : string.Join("; ", parts);
    }

    private static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
