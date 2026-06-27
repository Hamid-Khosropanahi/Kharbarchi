using Kharbarchi.Server.Contracts;
using Kharbarchi.Server.Data;
using Kharbarchi.Server.Models;
using Kharbarchi.Server.Security;
using Kharbarchi.Server.Services;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Shared.Models;
using Kharbarchi.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/admin/kharbarchi-control")]
[Authorize(Policy = AuthorizationPolicyNames.CatalogRead)]
[EnableRateLimiting("admin")]
[Produces("application/json")]
public sealed class KharbarchiControlCenterController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly KharbarchiPriceControlService _priceControl;
    private readonly KharbarchiWooCatalogSyncService _wooSync;
    private readonly WooCommerceApiClient _woo;

    public KharbarchiControlCenterController(
        AppDbContext context,
        KharbarchiPriceControlService priceControl,
        KharbarchiWooCatalogSyncService wooSync,
        WooCommerceApiClient woo)
    {
        _context = context;
        _priceControl = priceControl;
        _wooSync = wooSync;
        _woo = woo;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<KharbarchiControlSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        await EnsureMissingProfilesAsync(cancellationToken);

        var totalProducts = await _context.Products.CountAsync(cancellationToken);
        var green = await _context.ProductWooControlProfiles.CountAsync(x => x.PriceCheckStatus == "green", cancellationToken);
        var yellow = await _context.ProductWooControlProfiles.CountAsync(x => x.PriceCheckStatus == "yellow", cancellationToken);
        var red = await _context.ProductWooControlProfiles.CountAsync(x => x.PriceCheckStatus == "red", cancellationToken);
        var pending = await _context.ProductWooControlProfiles.CountAsync(x => x.WooSyncStatus != "synced" && x.WooSyncStatus != "failed", cancellationToken);
        var failed = await _context.ProductWooControlProfiles.CountAsync(x => x.WooSyncStatus == "failed", cancellationToken);
        var synced = await _context.ProductWooControlProfiles.CountAsync(x => x.WooSyncStatus == "synced", cancellationToken);
        var lowStock = await _context.Products.CountAsync(x => x.MinStockAlertQuantity.HasValue && x.StockQuantity <= x.MinStockAlertQuantity.Value, cancellationToken);
        var openOrders = await _context.WooCommerceOrderSnapshots.CountAsync(x => x.InternalStatus != WooOrderInternalStatus.Completed && x.InternalStatus != WooOrderInternalStatus.Cancelled, cancellationToken);
        var paymentPending = await _context.WooCommerceOrderSnapshots.CountAsync(x => x.PaymentStatus != PaymentStatusNames.Paid, cancellationToken);

        return Ok(new KharbarchiControlSummaryDto(totalProducts, green, yellow, red, pending, failed, synced, lowStock, openOrders, paymentPending));
    }

    [HttpGet("products")]
    public async Task<ActionResult<IReadOnlyList<KharbarchiProductControlListItemDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        await EnsureMissingProfilesAsync(cancellationToken);
        await RecalculateAllProfilesAsync(cancellationToken);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var query = ProductQuery().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x => x.Name.Contains(s) || x.Slug.Contains(s) || (x.Sku != null && x.Sku.Contains(s)) || x.Category!.Name.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.WooControlProfile != null && x.WooControlProfile.PriceCheckStatus == status);
        }

        var products = await query
            .OrderByDescending(x => x.WooControlProfile != null && x.WooControlProfile.PriceCheckStatus == "red")
            .ThenByDescending(x => x.WooControlProfile != null && x.WooControlProfile.PriceCheckStatus == "yellow")
            .ThenBy(x => x.Category!.Name)
            .ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = products.Select(ToListItem).ToList();
        return Ok(result);
    }

    [HttpGet("products/{id:int}")]
    public async Task<ActionResult<KharbarchiProductEditDto>> GetProduct(int id, CancellationToken cancellationToken)
    {
        var product = await ProductQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return NotFound();
        var profile = await EnsureProfileAsync(product, cancellationToken);
        _priceControl.Apply(product, profile);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToEditDto(product, profile));
    }

    [HttpPut("products/{id:int}")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogWrite)]
    public async Task<ActionResult<KharbarchiProductEditDto>> UpdateProduct(int id, [FromBody] KharbarchiProductUpdateRequest request, CancellationToken cancellationToken)
    {
        var product = await ProductQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return NotFound();

        product.Name = request.Name.Trim();
        product.Slug = SlugService.NormalizeSlug(request.Slug, request.Name);
        product.Sku = string.IsNullOrWhiteSpace(request.Sku) ? product.Sku : request.Sku.Trim();
        product.Description = request.Description?.Trim() ?? string.Empty;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.CommodityId = request.CommodityId;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.GalleryJson = request.GalleryJson;
        product.IsAvailable = request.IsAvailable;
        product.StockQuantity = Math.Max(0, request.StockQuantity);
        product.MinStockAlertQuantity = request.MinStockAlertQuantity;
        product.WooCommerceProductId = request.WooCommerceProductId;

        var profile = await EnsureProfileAsync(product, cancellationToken);
        ApplyRequest(profile, request);
        _priceControl.Apply(product, profile);
        profile.WooSyncStatus = "pending";
        profile.WooLastError = null;

        await _context.SaveChangesAsync(cancellationToken);
        await ReloadNavigationAsync(product, cancellationToken);
        return Ok(ToEditDto(product, profile));
    }

    [HttpPost("products/{id:int}/validate")]
    public async Task<ActionResult<KharbarchiProductEditDto>> ValidateProduct(int id, CancellationToken cancellationToken)
    {
        var product = await ProductQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return NotFound();
        var profile = await EnsureProfileAsync(product, cancellationToken);
        _priceControl.Apply(product, profile);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToEditDto(product, profile));
    }

    [HttpPost("products/{id:int}/sync-woocommerce")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogWrite)]
    public async Task<ActionResult<KharbarchiWooSyncItemResultDto>> SyncProduct(int id, CancellationToken cancellationToken)
    {
        return Ok(await _wooSync.SyncProductByIdAsync(id, cancellationToken));
    }

    [HttpPost("woocommerce/sync-batch")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogWrite)]
    public async Task<ActionResult<KharbarchiWooSyncResultDto>> SyncBatch([FromQuery] int take = 1, [FromQuery] bool pendingOnly = true, CancellationToken cancellationToken = default)
    {
        return Ok(await _wooSync.SyncAllAsync(take, pendingOnly, cancellationToken));
    }

    [HttpGet("woocommerce/progress")]
    public async Task<IActionResult> GetWooProgress(CancellationToken cancellationToken)
    {
        var progress = await _wooSync.GetProgressAsync(cancellationToken);
        return Ok(new { progress.Total, progress.Synced, progress.Failed, progress.Pending, progress.Percent });
    }

    [HttpPut("products/{id:int}/inventory")]
    [Authorize(Policy = AuthorizationPolicyNames.InventoryProposalManagerApproval)]
    public async Task<IActionResult> UpdateInventory(int id, [FromBody] KharbarchiInventoryUpdateRequest request, CancellationToken cancellationToken)
    {
        var product = await ProductQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return NotFound();

        product.StockQuantity = Math.Max(0, request.StockQuantity);
        product.MinStockAlertQuantity = request.MinStockAlertQuantity;
        product.UpdatedAtUtc = DateTime.UtcNow;
        var profile = await EnsureProfileAsync(product, cancellationToken);
        profile.WooSyncStatus = "pending";

        await _context.ApprovalAuditLogs.AddAsync(new ApprovalAuditLog
        {
            EntityType = "ProductInventory",
            EntityId = product.Id,
            Action = "ERP inventory updated",
            UserName = User.Identity?.Name ?? "unknown",
            UserRole = "Inventory",
            Note = request.Note
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        if (request.SendToWooCommerce)
        {
            return Ok(await _wooSync.SyncProductByIdAsync(id, cancellationToken));
        }

        return NoContent();
    }

    [HttpPut("products/{id:int}/price")]
    [Authorize(Policy = AuthorizationPolicyNames.PriceProposalFinalApproval)]
    public async Task<ActionResult<KharbarchiProductEditDto>> UpdatePrice(int id, [FromBody] KharbarchiPriceUpdateRequest request, CancellationToken cancellationToken)
    {
        var product = await ProductQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return NotFound();
        var profile = await EnsureProfileAsync(product, cancellationToken);

        profile.SaleCreditPrice = request.SaleCreditPrice ?? profile.SaleCreditPrice;
        profile.SaleCashPrice = request.SaleCashPrice ?? profile.SaleCashPrice;
        profile.BuyCreditPrice = request.BuyCreditPrice ?? profile.BuyCreditPrice;
        profile.BuyCashPrice = request.BuyCashPrice ?? profile.BuyCashPrice;
        profile.SaleCreditPricePerKg = request.SaleCreditPricePerKg ?? profile.SaleCreditPricePerKg;
        profile.SaleCashPricePerKg = request.SaleCashPricePerKg ?? profile.SaleCashPricePerKg;
        profile.BuyCreditPricePerKg = request.BuyCreditPricePerKg ?? profile.BuyCreditPricePerKg;
        profile.BuyCashPricePerKg = request.BuyCashPricePerKg ?? profile.BuyCashPricePerKg;
        profile.WooSyncStatus = "pending";
        _priceControl.Apply(product, profile);

        await _context.ApprovalAuditLogs.AddAsync(new ApprovalAuditLog
        {
            EntityType = "ProductPrice",
            EntityId = product.Id,
            Action = "ERP price updated",
            UserName = User.Identity?.Name ?? "unknown",
            UserRole = "Pricing",
            Note = request.Note
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        if (request.SendToWooCommerce && profile.PriceCheckStatus != "red")
        {
            await _wooSync.SyncProductByIdAsync(id, cancellationToken);
        }

        await ReloadNavigationAsync(product, cancellationToken);
        return Ok(ToEditDto(product, profile));
    }

    [HttpGet("orders")]
    [Authorize(Policy = AuthorizationPolicyNames.OrdersRead)]
    public async Task<ActionResult<IReadOnlyList<KharbarchiOrderControlListItemDto>>> GetOrders([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 40, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _context.WooCommerceOrderSnapshots.AsNoTracking().Include(x => x.ManualReceipts).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x => x.WooCommerceOrderNumber.Contains(s) || x.CustomerFullName.Contains(s) || (x.CustomerPhone != null && x.CustomerPhone.Contains(s)));
        }

        var orders = await query.OrderByDescending(x => x.WooCreatedAtUtc ?? x.SyncedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return Ok(orders.Select(ToOrderListItem).ToList());
    }

    [HttpPost("orders/{id:long}/confirm")]
    [Authorize(Policy = AuthorizationPolicyNames.OrderPaymentWorkflow)]
    public async Task<IActionResult> ConfirmOrder(long id, [FromBody] KharbarchiOrderConfirmRequest request, CancellationToken cancellationToken)
    {
        var order = await _context.WooCommerceOrderSnapshots.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null) return NotFound();
        order.InternalStatus = request.InternalStatus;
        order.LastActionByUserName = User.Identity?.Name ?? "unknown";
        order.LastActionNote = request.Note;
        order.ReadyToShipAtUtc = request.InternalStatus == WooOrderInternalStatus.ReadyToShip ? DateTime.UtcNow : order.ReadyToShipAtUtc;
        await _context.SaveChangesAsync(cancellationToken);

        if (request.SendToWooCommerce)
        {
            await _woo.UpdateOrderAsync(order.WooCommerceOrderId, new
            {
                status = MapWooStatus(request.InternalStatus),
                meta_data = new[]
                {
                    new WooMetaPayload("_khb_erp_internal_status", request.InternalStatus),
                    new WooMetaPayload("_khb_erp_confirmed_at_utc", DateTime.UtcNow.ToString("O")),
                    new WooMetaPayload("_khb_erp_confirmed_by", order.LastActionByUserName ?? string.Empty)
                }
            }, cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                await _woo.AddOrderNoteAsync(order.WooCommerceOrderId, new WooCommerceOrderNotePayload(request.Note, false, true), cancellationToken);
            }
        }

        return NoContent();
    }

    [HttpPost("orders/{id:long}/received-payment")]
    [Authorize(Policy = AuthorizationPolicyNames.OrderPaymentWorkflow)]
    public async Task<IActionResult> UpdateReceivedPayment(long id, [FromBody] KharbarchiOrderReceivedPaymentRequest request, CancellationToken cancellationToken)
    {
        var order = await _context.WooCommerceOrderSnapshots.Include(x => x.ManualReceipts).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null) return NotFound();

        var userName = User.Identity?.Name ?? "unknown";
        var receipt = new ManualPaymentReceipt
        {
            WooCommerceOrderSnapshotId = order.Id,
            WooCommerceOrderId = order.WooCommerceOrderId,
            Amount = request.ReceivedAmount,
            Currency = order.Currency ?? "IRR",
            ReceiptNumber = $"ERP-{order.WooCommerceOrderId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            PaymentSource = request.PaymentType,
            PaidAtUtc = DateTime.UtcNow,
            RegisteredByUserName = userName,
            Note = request.Note,
            SentToWooCommerce = false
        };
        _context.ManualPaymentReceipts.Add(receipt);
        order.PaymentStatus = request.ReceivedAmount >= order.TotalAmount ? PaymentStatusNames.Paid : "PartiallyPaid";
        order.LastActionByUserName = userName;
        order.LastActionNote = request.Note;
        await _context.SaveChangesAsync(cancellationToken);

        if (request.SendToWooCommerce)
        {
            var setPaid = request.SetPaidWhenFull && request.ReceivedAmount >= order.TotalAmount;
            await _woo.UpdateOrderAsync(order.WooCommerceOrderId, new
            {
                set_paid = setPaid,
                status = setPaid ? "processing" : order.WooCommerceStatus,
                meta_data = new[]
                {
                    new WooMetaPayload("_khb_erp_received_amount", request.ReceivedAmount.ToString("0", System.Globalization.CultureInfo.InvariantCulture)),
                    new WooMetaPayload("_khb_erp_payment_type", request.PaymentType),
                    new WooMetaPayload("_khb_erp_payment_status", order.PaymentStatus),
                    new WooMetaPayload("_khb_erp_payment_updated_at_utc", DateTime.UtcNow.ToString("O"))
                }
            }, cancellationToken);

            receipt.SentToWooCommerce = true;
            receipt.SentToWooCommerceAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }

    private IQueryable<Product> ProductQuery()
    {
        return _context.Products
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Commodity)
            .Include(x => x.WooControlProfile);
    }

    private async Task EnsureMissingProfilesAsync(CancellationToken cancellationToken)
    {
        var products = await _context.Products.Include(x => x.WooControlProfile).Where(x => x.WooControlProfile == null).Take(200).ToListAsync(cancellationToken);
        foreach (var product in products)
        {
            var profile = new ProductWooControlProfile
            {
                ProductId = product.Id,
                SaleCreditPrice = product.Price,
                SaleCashPrice = product.Price,
                BuyCreditPrice = product.PurchasePrice,
                BuyCashPrice = product.PurchasePrice,
                PackageGroup = "none",
                PackageCode = "NOPKG",
                PackageTitle = "بدون بسته‌بندی",
                SaleUnit = "item",
                WoodmartPriceUnitOfMeasure = "عدد",
                WooSyncStatus = product.WooCommerceProductId is > 0 ? "pending" : "pending"
            };
            product.WooControlProfile = profile;
            _context.ProductWooControlProfiles.Add(profile);
            _priceControl.Apply(product, profile);
        }
        if (products.Count > 0) await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<ProductWooControlProfile> EnsureProfileAsync(Product product, CancellationToken cancellationToken)
    {
        if (product.WooControlProfile is not null) return product.WooControlProfile;
        await EnsureMissingProfilesAsync(cancellationToken);
        await ReloadNavigationAsync(product, cancellationToken);
        return product.WooControlProfile ?? throw new InvalidOperationException("Control profile was not created.");
    }

    private async Task ReloadNavigationAsync(Product product, CancellationToken cancellationToken)
    {
        await _context.Entry(product).Reference(x => x.Category).LoadAsync(cancellationToken);
        await _context.Entry(product).Reference(x => x.Brand).LoadAsync(cancellationToken);
        await _context.Entry(product).Reference(x => x.Commodity).LoadAsync(cancellationToken);
        await _context.Entry(product).Reference(x => x.WooControlProfile).LoadAsync(cancellationToken);
    }

    private async Task RecalculateAllProfilesAsync(CancellationToken cancellationToken)
    {
        var products = await ProductQuery().Take(500).ToListAsync(cancellationToken);
        foreach (var product in products)
        {
            if (product.WooControlProfile is not null) _priceControl.Apply(product, product.WooControlProfile);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyRequest(ProductWooControlProfile profile, KharbarchiProductUpdateRequest request)
    {
        profile.PriceSourceMode = string.IsNullOrWhiteSpace(request.PriceSourceMode) ? "final_price" : request.PriceSourceMode.Trim();
        profile.PackageGroup = string.IsNullOrWhiteSpace(request.PackageGroup) ? "none" : request.PackageGroup.Trim();
        profile.PackageCode = request.PackageCode?.Trim();
        profile.PackageTitle = request.PackageTitle?.Trim();
        profile.ImageTag = request.ImageTag?.Trim();
        profile.UnitWeightKg = request.UnitWeightKg;
        profile.ProductCartonCount = request.ProductCartonCount;
        profile.BulkWeightKg = request.BulkWeightKg;
        profile.MinPurchaseKg = request.MinPurchaseKg;
        profile.MinCartons = request.MinCartons;
        profile.MaxCartons = request.MaxCartons;
        profile.CartonStep = request.CartonStep;
        profile.SaleUnit = request.SaleUnit;
        profile.WoodmartPriceUnitOfMeasure = request.WoodmartPriceUnitOfMeasure;
        profile.SaleCashPrice = request.SaleCashPrice;
        profile.SaleCreditPrice = request.SaleCreditPrice;
        profile.BuyCashPrice = request.BuyCashPrice;
        profile.BuyCreditPrice = request.BuyCreditPrice;
        profile.SaleCashPricePerKg = request.SaleCashPricePerKg;
        profile.SaleCreditPricePerKg = request.SaleCreditPricePerKg;
        profile.BuyCashPricePerKg = request.BuyCashPricePerKg;
        profile.BuyCreditPricePerKg = request.BuyCreditPricePerKg;
    }

    private KharbarchiProductControlListItemDto ToListItem(Product product)
    {
        var profile = product.WooControlProfile ?? new ProductWooControlProfile();
        var totalWeight = _priceControl.CalculateTotalWeight(profile);
        var saleCredit = profile.SaleCreditPrice ?? product.Price;
        return new KharbarchiProductControlListItemDto(
            product.Id,
            product.Name,
            product.Sku,
            product.Category?.Name ?? "بدون دسته",
            product.Brand?.Name,
            product.Commodity?.Name,
            saleCredit,
            profile.SaleCashPrice,
            profile.SaleCashPrice.HasValue ? saleCredit - profile.SaleCashPrice.Value : null,
            profile.SaleCreditPricePerKg,
            profile.SaleCashPricePerKg.HasValue && profile.SaleCreditPricePerKg.HasValue ? profile.SaleCreditPricePerKg.Value - profile.SaleCashPricePerKg.Value : null,
            totalWeight,
            product.StockQuantity,
            null,
            profile.PackageGroup,
            profile.PackageTitle,
            profile.PriceCheckStatus,
            profile.PriceCheckCode,
            profile.PriceCheckNote,
            profile.WooSyncStatus,
            product.WooCommerceProductId,
            product.IsAvailable);
    }

    private static KharbarchiProductEditDto ToEditDto(Product product, ProductWooControlProfile profile)
    {
        return new KharbarchiProductEditDto
        {
            ProductId = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Sku = product.Sku,
            Description = product.Description,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            CommodityId = product.CommodityId,
            ImageUrl = product.ImageUrl,
            GalleryJson = product.GalleryJson,
            IsAvailable = product.IsAvailable,
            StockQuantity = product.StockQuantity,
            MinStockAlertQuantity = product.MinStockAlertQuantity,
            WooCommerceProductId = product.WooCommerceProductId,
            PriceSourceMode = profile.PriceSourceMode,
            PackageGroup = profile.PackageGroup,
            PackageCode = profile.PackageCode,
            PackageTitle = profile.PackageTitle,
            ImageTag = profile.ImageTag,
            UnitWeightKg = profile.UnitWeightKg,
            ProductCartonCount = profile.ProductCartonCount,
            BulkWeightKg = profile.BulkWeightKg,
            MinPurchaseKg = profile.MinPurchaseKg,
            MinCartons = profile.MinCartons,
            MaxCartons = profile.MaxCartons,
            CartonStep = profile.CartonStep,
            SaleUnit = profile.SaleUnit,
            WoodmartPriceUnitOfMeasure = profile.WoodmartPriceUnitOfMeasure,
            SaleCashPrice = profile.SaleCashPrice,
            SaleCreditPrice = profile.SaleCreditPrice,
            BuyCashPrice = profile.BuyCashPrice,
            BuyCreditPrice = profile.BuyCreditPrice,
            SaleCashPricePerKg = profile.SaleCashPricePerKg,
            SaleCreditPricePerKg = profile.SaleCreditPricePerKg,
            BuyCashPricePerKg = profile.BuyCashPricePerKg,
            BuyCreditPricePerKg = profile.BuyCreditPricePerKg,
            ExpectedSaleCreditPrice = profile.ExpectedSaleCreditPrice,
            ExpectedSaleCashPrice = profile.ExpectedSaleCashPrice,
            ExpectedBuyCreditPrice = profile.ExpectedBuyCreditPrice,
            ExpectedBuyCashPrice = profile.ExpectedBuyCashPrice,
            PriceCheckAmount = profile.PriceCheckAmount,
            PriceCheckPercent = profile.PriceCheckPercent,
            PriceCheckStatus = profile.PriceCheckStatus,
            PriceCheckCode = profile.PriceCheckCode,
            PriceCheckNote = profile.PriceCheckNote,
            WooSyncStatus = profile.WooSyncStatus,
            WooLastError = profile.WooLastError
        };
    }

    private static KharbarchiOrderControlListItemDto ToOrderListItem(WooCommerceOrderSnapshot order)
    {
        var received = order.ManualReceipts.Sum(x => x.Amount);
        return new KharbarchiOrderControlListItemDto(
            order.Id,
            order.WooCommerceOrderId,
            order.WooCommerceOrderNumber,
            order.WooCommerceStatus,
            order.InternalStatus,
            order.PaymentStatus,
            order.CustomerFullName,
            order.CustomerPhone,
            order.TotalAmount,
            received,
            order.Currency,
            order.WooCreatedAtUtc);
    }

    private static string MapWooStatus(string internalStatus)
    {
        return internalStatus switch
        {
            WooOrderInternalStatus.Completed => "completed",
            WooOrderInternalStatus.Cancelled => "cancelled",
            WooOrderInternalStatus.ReadyToShip or WooOrderInternalStatus.PaymentConfirmed or WooOrderInternalStatus.Shipped => "processing",
            WooOrderInternalStatus.PaymentFailed => "failed",
            _ => "on-hold"
        };
    }
}
