using System.Globalization;
using System.Text.Json;
using Kharbarchi.Server.Contracts;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Server.Data;
using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Services;

public sealed class WooCommerceImportService
{
    private readonly AppDbContext _context;
    private readonly WooCommerceApiClient _woo;
    private readonly ILogger<WooCommerceImportService> _logger;

    public WooCommerceImportService(AppDbContext context, WooCommerceApiClient woo, ILogger<WooCommerceImportService> logger)
    {
        _context = context;
        _woo = woo;
        _logger = logger;
    }

    public async Task<ImportResult> ImportProductsAsync(ImportWooCommerceProductsRequest request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var maxPages = Math.Clamp(request.MaxPages, 1, 1000);
        var scanned = 0;
        var created = 0;
        var updated = 0;
        var skipped = 0;
        var messages = new List<string>();

        for (var page = 1; page <= maxPages; page++)
        {
            using var document = await _woo.GetProductsAsync(page, pageSize, cancellationToken);
            if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
            {
                break;
            }

            foreach (var productJson in document.RootElement.EnumerateArray())
            {
                scanned++;
                var wooId = GetLong(productJson, "id");
                if (wooId is null or <= 0)
                {
                    skipped++;
                    continue;
                }

                var name = GetString(productJson, "name") ?? $"Woo Product {wooId}";
                var slug = GetString(productJson, "slug") ?? $"woo-product-{wooId}";
                var sku = GetString(productJson, "sku");
                var status = GetString(productJson, "status") ?? "publish";
                var price = GetDecimal(productJson, "regular_price") ?? GetDecimal(productJson, "price") ?? 0;
                var salePrice = GetDecimal(productJson, "sale_price");
                var stockQuantity = GetInt(productJson, "stock_quantity") ?? 0;

                var categoryId = await EnsureImportedCategoryAsync(productJson, request.DryRun, cancellationToken);

                var local = await _context.Products
                    .Include(x => x.Variants)
                    .FirstOrDefaultAsync(x => x.WooCommerceProductId == wooId, cancellationToken);

                if (local is null)
                {
                    created++;
                    if (!request.DryRun)
                    {
                        local = new Product
                        {
                            WooCommerceProductId = wooId,
                            Name = name,
                            Slug = slug,
                            Sku = string.IsNullOrWhiteSpace(sku) ? $"woo-{wooId}" : sku,
                            CategoryId = categoryId,
                            Description = GetString(productJson, "description") ?? string.Empty,
                            Price = salePrice is > 0 ? salePrice.Value : price,
                            OldPrice = salePrice is > 0 && price > salePrice ? price : null,
                            StockQuantity = stockQuantity,
                            IsAvailable = string.Equals(status, "publish", StringComparison.OrdinalIgnoreCase),
                            ImageUrl = ExtractImageUrl(productJson),
                            UpdatedAtUtc = DateTime.UtcNow
                        };
                        _context.Products.Add(local);
                    }
                }
                else
                {
                    updated++;
                    if (!request.DryRun)
                    {
                        local.Name = name;
                        local.Slug = slug;
                        local.Sku = string.IsNullOrWhiteSpace(sku) ? local.Sku : sku;
                        local.CategoryId = categoryId;
                        local.Description = GetString(productJson, "description") ?? local.Description;
                        local.Price = salePrice is > 0 ? salePrice.Value : price;
                        local.OldPrice = salePrice is > 0 && price > salePrice ? price : null;
                        local.StockQuantity = stockQuantity;
                        local.IsAvailable = string.Equals(status, "publish", StringComparison.OrdinalIgnoreCase);
                        local.ImageUrl = ExtractImageUrl(productJson) ?? local.ImageUrl;
                        local.UpdatedAtUtc = DateTime.UtcNow;
                    }
                }

                if (!request.DryRun)
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            if (document.RootElement.GetArrayLength() < pageSize)
            {
                break;
            }
        }

        messages.Add(request.DryRun ? "DryRun انجام شد؛ هیچ تغییری در دیتابیس ثبت نشد." : "محصولات WooCommerce در دیتابیس لوکال همگام شدند.");
        return new ImportResult(scanned, created, updated, skipped, messages);
    }

    public async Task<ImportResult> ImportOrdersAsync(ImportWooCommerceOrdersRequest request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var maxPages = Math.Clamp(request.MaxPages, 1, 1000);
        var scanned = 0;
        var created = 0;
        var updated = 0;
        var skipped = 0;
        var messages = new List<string>();

        for (var page = 1; page <= maxPages; page++)
        {
            using var document = await _woo.GetOrdersAsync(request.Status, request.AfterUtc, page, pageSize, cancellationToken);
            if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
            {
                break;
            }

            foreach (var orderJson in document.RootElement.EnumerateArray())
            {
                scanned++;
                var wooOrderId = GetLong(orderJson, "id");
                if (wooOrderId is null or <= 0)
                {
                    skipped++;
                    continue;
                }

                var local = await _context.WooCommerceOrderSnapshots
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.WooCommerceOrderId == wooOrderId.Value, cancellationToken);

                if (local is null)
                {
                    created++;
                    if (!request.DryRun)
                    {
                        local = new WooCommerceOrderSnapshot { WooCommerceOrderId = wooOrderId.Value };
                        _context.WooCommerceOrderSnapshots.Add(local);
                    }
                }
                else
                {
                    updated++;
                }

                if (!request.DryRun && local is not null)
                {
                    MapOrder(orderJson, local);
                    ReplaceOrderItems(orderJson, local);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            if (document.RootElement.GetArrayLength() < pageSize)
            {
                break;
            }
        }

        messages.Add(request.DryRun ? "DryRun انجام شد؛ هیچ سفارشی در دیتابیس ثبت نشد." : "سفارش‌ها و اقلام سفارش در دیتابیس لوکال به‌روزرسانی شدند.");
        return new ImportResult(scanned, created, updated, skipped, messages);
    }

    public async Task<IReadOnlyList<LocalWooOrderListItemDto>> GetOrdersAsync(string? internalStatus, string? paymentStatus, string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.WooCommerceOrderSnapshots.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(internalStatus))
        {
            query = query.Where(x => x.InternalStatus == internalStatus);
        }

        if (!string.IsNullOrWhiteSpace(paymentStatus))
        {
            query = query.Where(x => x.PaymentStatus == paymentStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x => x.WooCommerceOrderNumber.Contains(s) || x.CustomerFullName.Contains(s) || (x.CustomerPhone != null && x.CustomerPhone.Contains(s)));
        }

        return await query
            .OrderByDescending(x => x.WooCreatedAtUtc ?? x.SyncedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new LocalWooOrderListItemDto(
                x.Id,
                x.WooCommerceOrderId,
                x.WooCommerceOrderNumber,
                x.WooCommerceStatus,
                x.InternalStatus,
                x.PaymentStatus,
                x.CustomerFullName,
                x.CustomerPhone,
                x.TotalAmount,
                x.Currency,
                x.WooCreatedAtUtc,
                x.SyncedAtUtc,
                x.Items.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<LocalWooOrderDetailsDto?> GetOrderDetailsAsync(long id, CancellationToken cancellationToken)
    {
        var order = await _context.WooCommerceOrderSnapshots
            .AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.BarookPayments)
            .Include(x => x.ManualReceipts)
            .FirstOrDefaultAsync(x => x.Id == id || x.WooCommerceOrderId == id, cancellationToken);

        return order is null ? null : ToDetailsDto(order);
    }

    public async Task<bool> ChangeInternalStatusAsync(long id, ChangeInternalOrderStatusRequest request, string userName, CancellationToken cancellationToken)
    {
        var order = await _context.WooCommerceOrderSnapshots.FirstOrDefaultAsync(x => x.Id == id || x.WooCommerceOrderId == id, cancellationToken);
        if (order is null)
        {
            return false;
        }

        order.InternalStatus = request.InternalStatus.Trim();
        order.PaymentStatus = order.InternalStatus == WooOrderInternalStatus.AwaitingPayment ? PaymentStatusNames.AwaitingPayment : order.PaymentStatus;
        order.LastActionByUserName = userName;
        order.LastActionNote = request.Note?.Trim();
        order.SyncedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        if (request.UpdateWooCommerceNote)
        {
            await _woo.AddOrderNoteAsync(order.WooCommerceOrderId,
                new WooCommerceOrderNotePayload($"Kharbarchi internal status changed to {order.InternalStatus} by {userName}. Note: {request.Note}"),
                cancellationToken);
        }

        return true;
    }

    private async Task<int> EnsureImportedCategoryAsync(JsonElement productJson, bool dryRun, CancellationToken cancellationToken)
    {
        var categoryName = "وارد شده از ووکامرس";
        var categorySlug = "imported-from-woocommerce";

        if (productJson.TryGetProperty("categories", out var categories) && categories.ValueKind == JsonValueKind.Array && categories.GetArrayLength() > 0)
        {
            var first = categories[0];
            categoryName = GetString(first, "name") ?? categoryName;
            categorySlug = GetString(first, "slug") ?? categorySlug;
        }

        var existing = await _context.Categories.FirstOrDefaultAsync(x => x.Slug == categorySlug, cancellationToken);
        if (existing is not null)
        {
            return existing.Id;
        }

        if (dryRun)
        {
            return 1;
        }

        var category = new Category { Name = categoryName, Slug = categorySlug };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        return category.Id;
    }

    private static void MapOrder(JsonElement orderJson, WooCommerceOrderSnapshot order)
    {
        var billing = TryGetObject(orderJson, "billing");
        var shipping = TryGetObject(orderJson, "shipping");
        var status = GetString(orderJson, "status") ?? order.WooCommerceStatus;

        order.WooCommerceOrderNumber = GetString(orderJson, "number") ?? order.WooCommerceOrderId.ToString(CultureInfo.InvariantCulture);
        order.WooCommerceStatus = status;
        order.InternalStatus = MapDefaultInternalStatus(status, order.InternalStatus);
        order.PaymentStatus = Bool(orderJson, "date_paid") ? PaymentStatusNames.Paid : order.PaymentStatus;
        order.PaymentMethod = GetString(orderJson, "payment_method");
        order.PaymentMethodTitle = GetString(orderJson, "payment_method_title");
        order.TransactionId = GetString(orderJson, "transaction_id");
        order.Currency = GetString(orderJson, "currency") ?? "IRR";
        order.TotalAmount = GetDecimal(orderJson, "total") ?? 0;
        order.ShippingTotal = GetDecimal(orderJson, "shipping_total") ?? 0;
        order.DiscountTotal = GetDecimal(orderJson, "discount_total") ?? 0;
        order.CustomerFullName = Combine(GetString(billing, "first_name"), GetString(billing, "last_name"));
        order.CustomerPhone = GetString(billing, "phone");
        order.CustomerEmail = GetString(billing, "email");
        order.CustomerNationalCode = ExtractNationalCode(orderJson);
        order.BillingAddress = BuildAddress(billing);
        order.ShippingAddress = BuildAddress(shipping);
        order.CustomerNote = GetString(orderJson, "customer_note");
        order.WooCreatedAtUtc = GetDateTime(orderJson, "date_created_gmt") ?? GetDateTime(orderJson, "date_created");
        order.WooUpdatedAtUtc = GetDateTime(orderJson, "date_modified_gmt") ?? GetDateTime(orderJson, "date_modified");
        order.SyncedAtUtc = DateTime.UtcNow;
        order.RawJson = orderJson.GetRawText();
    }

    private static void ReplaceOrderItems(JsonElement orderJson, WooCommerceOrderSnapshot order)
    {
        order.Items.Clear();
        if (!orderJson.TryGetProperty("line_items", out var lineItems) || lineItems.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var line in lineItems.EnumerateArray())
        {
            var quantity = GetDecimal(line, "quantity") ?? 0;
            var lineTotal = GetDecimal(line, "total") ?? 0;
            order.Items.Add(new WooCommerceOrderItemSnapshot
            {
                WooCommerceLineItemId = GetLong(line, "id") ?? 0,
                WooCommerceProductId = GetLong(line, "product_id"),
                WooCommerceVariationId = GetLong(line, "variation_id"),
                Sku = GetString(line, "sku"),
                Name = GetString(line, "name") ?? "کالا",
                UnitType = "کارتن",
                Quantity = quantity,
                LineTotal = lineTotal,
                UnitPrice = quantity == 0 ? lineTotal : Math.Round(lineTotal / quantity, 2),
                RawJson = line.GetRawText()
            });
        }
    }

    private static LocalWooOrderDetailsDto ToDetailsDto(WooCommerceOrderSnapshot order)
    {
        return new LocalWooOrderDetailsDto(
            order.Id,
            order.WooCommerceOrderId,
            order.WooCommerceOrderNumber,
            order.WooCommerceStatus,
            order.InternalStatus,
            order.PaymentStatus,
            order.CustomerFullName,
            order.CustomerPhone,
            order.CustomerNationalCode,
            order.BillingAddress,
            order.ShippingAddress,
            order.CustomerNote,
            order.TotalAmount,
            order.Currency,
            order.Items.OrderBy(x => x.Id).Select(x => new LocalWooOrderItemDto(x.Id, x.WooCommerceLineItemId, x.WooCommerceProductId, x.WooCommerceVariationId, x.Sku, x.Name, x.Quantity, x.UnitType, x.UnitPrice, x.LineTotal)).ToList(),
            order.BarookPayments.OrderByDescending(x => x.Id).Select(x => new BarookPaymentSessionDto(x.Id, x.ExternalCode, x.Amount, x.Token, x.PaymentUrl, x.LinkSentAtUtc, x.BarookStatus, x.ReferenceNumber, x.MaskedCardNumber, x.TransactionId, x.PaidAtUtc, x.IsCompleted, x.CreatedAtUtc)).ToList(),
            order.ManualReceipts.OrderByDescending(x => x.Id).Select(x => new ManualPaymentReceiptDto(x.Id, x.Amount, x.Currency, x.ReceiptNumber, x.PaymentSource, x.PaidAtUtc, x.RegisteredByUserName, x.Note, x.SentToWooCommerce, x.CreatedAtUtc)).ToList());
    }

    private static string MapDefaultInternalStatus(string wooStatus, string current)
    {
        if (!string.IsNullOrWhiteSpace(current) && current != WooOrderInternalStatus.Synced)
        {
            return current;
        }

        return wooStatus.ToLowerInvariant() switch
        {
            "pending" => WooOrderInternalStatus.AwaitingPayment,
            "processing" => WooOrderInternalStatus.ReadyToShip,
            "completed" => WooOrderInternalStatus.Completed,
            "cancelled" or "canceled" => WooOrderInternalStatus.Cancelled,
            _ => WooOrderInternalStatus.Synced
        };
    }

    private static string? ExtractImageUrl(JsonElement productJson)
    {
        if (!productJson.TryGetProperty("images", out var images) || images.ValueKind != JsonValueKind.Array || images.GetArrayLength() == 0)
        {
            return null;
        }

        return GetString(images[0], "src");
    }

    private static string? ExtractNationalCode(JsonElement orderJson)
    {
        if (!orderJson.TryGetProperty("meta_data", out var meta) || meta.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var possibleKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "national_code",
            "billing_national_code",
            "_billing_national_code",
            "melli_code",
            "codemelli",
            "_khb_national_code"
        };

        foreach (var item in meta.EnumerateArray())
        {
            var key = GetString(item, "key");
            if (key is not null && possibleKeys.Contains(key))
            {
                var value = GetString(item, "value");
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }
        }

        return null;
    }

    private static JsonElement TryGetObject(JsonElement parent, string name)
    {
        return parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object
            ? value
            : default;
    }

    private static string BuildAddress(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        var parts = new[]
        {
            GetString(element, "state"),
            GetString(element, "city"),
            GetString(element, "address_1"),
            GetString(element, "address_2"),
            GetString(element, "postcode")
        };

        return string.Join(" - ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string Combine(string? first, string? last)
    {
        var fullName = string.Join(' ', new[] { first, last }.Where(x => !string.IsNullOrWhiteSpace(x)));
        return string.IsNullOrWhiteSpace(fullName) ? "مشتری ووکامرس" : fullName;
    }

    private static bool Bool(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var p) && p.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null
            ? property.ToString()
            : null;
    }

    private static long? GetLong(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property)) return null;
        return property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var v) ? v : long.TryParse(property.ToString(), out var parsed) ? parsed : null;
    }

    private static int? GetInt(JsonElement element, string propertyName)
    {
        var value = GetLong(element, propertyName);
        return value.HasValue ? (int)value.Value : null;
    }

    private static decimal? GetDecimal(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property)) return null;
        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var numeric)) return numeric;
        return decimal.TryParse(property.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }

    private static DateTime? GetDateTime(JsonElement element, string propertyName)
    {
        var value = GetString(element, propertyName);
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed) ? parsed.ToUniversalTime() : null;
    }
}
