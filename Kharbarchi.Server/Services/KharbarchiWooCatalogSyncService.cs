using System.Globalization;
using System.Text.Json;
using Kharbarchi.Server.Data;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Services;

public sealed class KharbarchiWooCatalogSyncService
{
    private readonly AppDbContext _context;
    private readonly WooCommerceApiClient _woo;
    private readonly KharbarchiPriceControlService _priceControl;
    private readonly KharbarchiWooPayloadFactory _payloadFactory;
    private readonly ILogger<KharbarchiWooCatalogSyncService> _logger;

    public KharbarchiWooCatalogSyncService(
        AppDbContext context,
        WooCommerceApiClient woo,
        KharbarchiPriceControlService priceControl,
        KharbarchiWooPayloadFactory payloadFactory,
        ILogger<KharbarchiWooCatalogSyncService> logger)
    {
        _context = context;
        _woo = woo;
        _priceControl = priceControl;
        _payloadFactory = payloadFactory;
        _logger = logger;
    }

    public async Task<KharbarchiWooSyncResultDto> SyncAllAsync(int take, bool pendingOnly, CancellationToken cancellationToken)
    {
        take = Math.Clamp(take, 1, 500);
        var query = ProductQuery()
            .OrderByDescending(x => x.WooControlProfile != null && x.WooControlProfile.PriceCheckStatus == "yellow")
            .ThenBy(x => x.WooControlProfile != null && x.WooControlProfile.PriceCheckStatus == "red")
            .ThenBy(x => x.Id)
            .AsQueryable();

        if (pendingOnly)
        {
            query = query.Where(x => x.WooControlProfile == null || x.WooControlProfile.WooSyncStatus != "synced");
        }

        var totalCandidates = await query.CountAsync(cancellationToken);
        var products = await query.Take(take).ToListAsync(cancellationToken);
        var items = new List<KharbarchiWooSyncItemResultDto>();
        var synced = 0;
        var skipped = 0;
        var failed = 0;

        foreach (var product in products)
        {
            var item = await SyncProductAsync(product, cancellationToken);
            items.Add(item);
            if (item.Status == "synced") synced++;
            else if (item.Status == "skipped") skipped++;
            else failed++;
        }

        var progress = await GetProgressAsync(cancellationToken);
        return new KharbarchiWooSyncResultDto(totalCandidates, synced, skipped, failed, progress.Pending, progress.Percent, items);
    }

    public async Task<KharbarchiWooSyncItemResultDto> SyncProductByIdAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await ProductQuery().FirstOrDefaultAsync(x => x.Id == productId, cancellationToken)
            ?? throw new InvalidOperationException("Product was not found.");

        return await SyncProductAsync(product, cancellationToken);
    }

    public async Task<(int Total, int Synced, int Failed, int Pending, int Percent)> GetProgressAsync(CancellationToken cancellationToken)
    {
        var total = await _context.Products.CountAsync(cancellationToken);
        var synced = await _context.ProductWooControlProfiles.CountAsync(x => x.WooSyncStatus == "synced", cancellationToken);
        var failed = await _context.ProductWooControlProfiles.CountAsync(x => x.WooSyncStatus == "failed", cancellationToken);
        var pending = Math.Max(0, total - synced - failed);
        var percent = total == 0 ? 0 : (int)Math.Round(synced * 100.0 / total);
        return (total, synced, failed, pending, percent);
    }

    private async Task<KharbarchiWooSyncItemResultDto> SyncProductAsync(Product product, CancellationToken cancellationToken)
    {
        var profile = await EnsureProfileAsync(product, cancellationToken);
        var snapshot = _priceControl.Apply(product, profile);

        if (profile.PriceCheckStatus == "red")
        {
            profile.WooSyncStatus = "skipped";
            profile.WooLastError = "Product has red control status and was not published to WooCommerce.";
            await _context.SaveChangesAsync(cancellationToken);
            return new KharbarchiWooSyncItemResultDto(product.Id, product.Name, product.Sku, product.WooCommerceProductId, "skipped", profile.WooLastError, snapshot.Status);
        }

        try
        {
            var wooCategoryId = await EnsureWooCategoryAsync(product.Category, cancellationToken);
            await TrySyncCommodityAsync(product.Commodity, product.Category, cancellationToken);
            await TrySyncBrandAsync(product.Brand, cancellationToken);
            await TrySyncPackageAsync(profile, cancellationToken);

            var payload = _payloadFactory.BuildProductPayload(product, profile, wooCategoryId);
            using var document = product.WooCommerceProductId is > 0
                ? await _woo.UpdateProductAsync(product.WooCommerceProductId.Value, payload, cancellationToken)
                : await _woo.CreateProductAsync(payload, cancellationToken);

            var wooProductId = GetLong(document.RootElement, "id");
            if (wooProductId is > 0)
            {
                product.WooCommerceProductId = wooProductId.Value;
                await TryLinkProductToCommodityAsync(wooProductId.Value, product.Commodity, cancellationToken);
            }

            profile.WooSyncStatus = "synced";
            profile.WooLastError = null;
            profile.WooSyncedAtUtc = DateTime.UtcNow;
            product.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return new KharbarchiWooSyncItemResultDto(product.Id, product.Name, product.Sku, product.WooCommerceProductId, "synced", "Product synced successfully.", snapshot.Status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Product Woo sync failed. ProductId={ProductId}", product.Id);
            profile.WooSyncStatus = "failed";
            profile.WooLastError = ex.Message;
            await _context.SaveChangesAsync(cancellationToken);
            return new KharbarchiWooSyncItemResultDto(product.Id, product.Name, product.Sku, product.WooCommerceProductId, "failed", ex.Message, snapshot.Status);
        }
    }

    private IQueryable<Product> ProductQuery()
    {
        return _context.Products
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Commodity)
            .Include(x => x.WooControlProfile);
    }

    private async Task<ProductWooControlProfile> EnsureProfileAsync(Product product, CancellationToken cancellationToken)
    {
        if (product.WooControlProfile is not null)
        {
            return product.WooControlProfile;
        }

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
            WooSyncStatus = "pending"
        };
        product.WooControlProfile = profile;
        _context.ProductWooControlProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);
        return profile;
    }

    private async Task<long?> EnsureWooCategoryAsync(Category? category, CancellationToken cancellationToken)
    {
        if (category is null) return null;
        if (category.WooCommerceCategoryId is > 0) return category.WooCommerceCategoryId.Value;

        using var found = await _woo.GetProductCategoriesBySlugAsync(category.Slug, cancellationToken);
        if (found.RootElement.ValueKind == JsonValueKind.Array && found.RootElement.GetArrayLength() > 0)
        {
            var id = GetLong(found.RootElement[0], "id");
            if (id is > 0)
            {
                category.WooCommerceCategoryId = id.Value;
                await _context.SaveChangesAsync(cancellationToken);
                return id.Value;
            }
        }

        using var created = await _woo.CreateProductCategoryAsync(new { name = category.Name, slug = category.Slug }, cancellationToken);
        var createdId = GetLong(created.RootElement, "id");
        if (createdId is > 0)
        {
            category.WooCommerceCategoryId = createdId.Value;
            await _context.SaveChangesAsync(cancellationToken);
        }
        return createdId;
    }

    private async Task TrySyncCommodityAsync(Commodity? commodity, Category? category, CancellationToken cancellationToken)
    {
        if (commodity is null) return;
        try
        {
            using var response = await _woo.PostWordPressEndpointAsync("wp-json/khb/v1/commodity/upsert", new
            {
                source_key = $"erp-commodity-{commodity.Id}",
                name = commodity.Name,
                slug = commodity.Slug,
                english_name = commodity.EnglishName ?? commodity.Slug,
                category_slug = category?.Slug ?? string.Empty
            }, cancellationToken);
            var id = GetLong(response.RootElement, "id") ?? GetLong(response.RootElement, "term_id");
            if (id is > 0)
            {
                commodity.WooCommerceCommodityId = id.Value;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogInformation(ex, "Commodity endpoint is not available; continuing with product meta only.");
        }
    }

    private async Task TrySyncBrandAsync(Brand? brand, CancellationToken cancellationToken)
    {
        if (brand is null) return;
        try
        {
            using var response = await _woo.PostWordPressEndpointAsync("wp-json/khb/v1/brand/upsert", new
            {
                source_key = $"erp-brand-{brand.Id}",
                name = brand.Name,
                slug = brand.Slug,
                logo_url = brand.LogoUrl ?? string.Empty
            }, cancellationToken);
            var id = GetLong(response.RootElement, "id") ?? GetLong(response.RootElement, "term_id");
            if (id is > 0)
            {
                brand.WooCommerceBrandId = id.Value;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogInformation(ex, "Brand endpoint is not available; continuing with product attributes/meta only.");
        }
    }

    private async Task TrySyncPackageAsync(ProductWooControlProfile profile, CancellationToken cancellationToken)
    {
        try
        {
            using var _ = await _woo.PostWordPressEndpointAsync("wp-json/khb/v1/package/upsert", new
            {
                source_key = $"erp-package-{profile.PackageCode ?? profile.Id.ToString(CultureInfo.InvariantCulture)}",
                title = profile.PackageTitle ?? profile.PackageCode ?? "Package",
                package_code = profile.PackageCode ?? "NOPKG",
                package_group = profile.PackageGroup,
                unit_weight = profile.UnitWeightKg ?? 0,
                default_carton_count = profile.ProductCartonCount ?? 0,
                image_tag = profile.ImageTag ?? string.Empty
            }, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogInformation(ex, "Package endpoint is not available; continuing with product meta only.");
        }
    }

    private async Task TryLinkProductToCommodityAsync(long wooProductId, Commodity? commodity, CancellationToken cancellationToken)
    {
        if (commodity?.WooCommerceCommodityId is not > 0) return;
        try
        {
            using var _ = await _woo.PostWordPressEndpointAsync($"wp-json/khb/v1/product/{wooProductId}/commodity", new
            {
                commodity_id = commodity.WooCommerceCommodityId.Value,
                commodity_slug = commodity.Slug
            }, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogInformation(ex, "Product commodity link endpoint is not available.");
        }
    }

    private static long? GetLong(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property)) return null;
        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var numeric)) return numeric;
        return long.TryParse(property.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }
}
