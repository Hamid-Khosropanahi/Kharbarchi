using Kharbarchi.Server.Data;
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
[Authorize(Policy = AuthorizationPolicyNames.CatalogRead)]
[EnableRateLimiting("admin")]
[Route("api/admin/catalog")]
[Produces("application/json")]
public sealed class CatalogAdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public CatalogAdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("lookups")]
    public async Task<ActionResult<CatalogLookupsDto>> GetLookups(CancellationToken cancellationToken)
    {
        var result = new CatalogLookupsDto(
            await _context.Categories.AsNoTracking().OrderBy(x => x.Name).Select(x => new LookupItemDto(x.Id, x.Name, x.Slug)).ToListAsync(cancellationToken),
            await _context.Brands.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).Select(x => new LookupItemDto(x.Id, x.Name, x.Slug)).ToListAsync(cancellationToken),
            await _context.Commodities.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).Select(x => new LookupItemDto(x.Id, x.Name, x.Slug)).ToListAsync(cancellationToken),
            await _context.ProductTags.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).Select(x => new LookupItemDto(x.Id, x.Name, x.Slug)).ToListAsync(cancellationToken),
            await _context.ProductSpecDefinitions.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ThenBy(x => x.Name).Select(x => new SpecDefinitionDto(x.Id, x.Name, x.Slug, x.Unit, x.SortOrder)).ToListAsync(cancellationToken));

        return Ok(result);
    }

    [HttpGet("products")]
    public async Task<ActionResult<IReadOnlyList<AdminProductListItemDto>>> GetProducts(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var canSeePurchasePrice = User.IsInRole(KharbarchiRoles.SuperAdmin) || User.IsInRole(KharbarchiRoles.LegacyAdmin) || User.IsInRole(KharbarchiRoles.PricingManager);
        var canSeePrice = canSeePurchasePrice || User.IsInRole(KharbarchiRoles.PricingEmployee);

        var query = _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Commodity)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Slug.Contains(term) || (x.Sku != null && x.Sku.Contains(term)));
        }

        var products = await query
            .OrderBy(x => x.Category!.Name)
            .ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminProductListItemDto(
                x.Id,
                x.Name,
                x.Sku,
                x.Category!.Name,
                x.Brand != null ? x.Brand.Name : null,
                x.Commodity != null ? x.Commodity.Name : null,
                canSeePrice ? x.Price : null,
                canSeePurchasePrice ? x.PurchasePrice : null,
                x.StockQuantity,
                x.IsAvailable,
                x.WooCommerceProductId))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("products/{id:int}")]
    public async Task<ActionResult<AdminProductDetailsDto>> GetProduct(int id, CancellationToken cancellationToken)
    {
        var canSeePurchasePrice = User.IsInRole(KharbarchiRoles.SuperAdmin) || User.IsInRole(KharbarchiRoles.LegacyAdmin) || User.IsInRole(KharbarchiRoles.PricingManager);

        var product = await _context.Products
            .AsNoTracking()
            .Include(x => x.Variants)
            .Include(x => x.ProductTags)
            .Include(x => x.SpecValues)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(new AdminProductDetailsDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Sku = product.Sku,
            Description = product.Description,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            CommodityId = product.CommodityId,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            OldPrice = product.OldPrice,
            PurchasePrice = canSeePurchasePrice ? product.PurchasePrice : null,
            StockQuantity = product.StockQuantity,
            MinStockAlertQuantity = product.MinStockAlertQuantity,
            IsAvailable = product.IsAvailable,
            WooCommerceProductId = product.WooCommerceProductId,
            TagIds = product.ProductTags.Select(x => x.ProductTagId).ToList(),
            Specs = product.SpecValues.Select(x => new ProductSpecValueUpsertDto { SpecDefinitionId = x.SpecDefinitionId, Value = x.Value }).ToList(),
            Variants = product.Variants.Select(x => new ProductVariantUpsertDto
            {
                Id = x.Id,
                Name = x.Name,
                Sku = x.Sku,
                Price = x.Price,
                OldPrice = x.OldPrice,
                PurchasePrice = canSeePurchasePrice ? x.PurchasePrice : null,
                StockQuantity = x.StockQuantity,
                MinStockAlertQuantity = x.MinStockAlertQuantity,
                IsDefault = x.IsDefault,
                IsAvailable = x.IsAvailable,
                WooCommerceProductId = x.WooCommerceProductId,
                WooCommerceVariationId = x.WooCommerceVariationId
            }).ToList()
        });
    }

    [HttpPost("brands")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogWrite)]
    public async Task<ActionResult<LookupItemDto>> CreateBrand([FromBody] BrandUpsertRequest request, CancellationToken cancellationToken)
    {
        var entity = new Brand
        {
            Name = request.Name.Trim(),
            Slug = SlugService.NormalizeSlug(request.Slug, request.Name),
            LogoUrl = request.LogoUrl?.Trim(),
            IsActive = request.IsActive
        };

        _context.Brands.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("Brand", entity.Id, "Create", null, cancellationToken);
        return CreatedAtAction(nameof(GetLookups), routeValues: null, value: new LookupItemDto(entity.Id, entity.Name, entity.Slug));
    }

    [HttpPost("commodities")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogWrite)]
    public async Task<ActionResult<LookupItemDto>> CreateCommodity([FromBody] CommodityUpsertRequest request, CancellationToken cancellationToken)
    {
        var entity = new Commodity
        {
            Name = request.Name.Trim(),
            Slug = SlugService.NormalizeSlug(request.Slug, request.EnglishName ?? request.Name),
            EnglishName = request.EnglishName?.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive
        };

        _context.Commodities.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("Commodity", entity.Id, "Create", null, cancellationToken);
        return CreatedAtAction(nameof(GetLookups), routeValues: null, value: new LookupItemDto(entity.Id, entity.Name, entity.Slug));
    }

    [HttpPost("tags")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogWrite)]
    public async Task<ActionResult<LookupItemDto>> CreateTag([FromBody] ProductTagUpsertRequest request, CancellationToken cancellationToken)
    {
        var entity = new ProductTag
        {
            Name = request.Name.Trim(),
            Slug = SlugService.NormalizeSlug(request.Slug, request.Name),
            IsActive = request.IsActive
        };

        _context.ProductTags.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("ProductTag", entity.Id, "Create", null, cancellationToken);
        return CreatedAtAction(nameof(GetLookups), routeValues: null, value: new LookupItemDto(entity.Id, entity.Name, entity.Slug));
    }

    [HttpPost("spec-definitions")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogWrite)]
    public async Task<ActionResult<SpecDefinitionDto>> CreateSpecDefinition([FromBody] SpecDefinitionUpsertRequest request, CancellationToken cancellationToken)
    {
        var entity = new ProductSpecDefinition
        {
            Name = request.Name.Trim(),
            Slug = SlugService.NormalizeSlug(request.Slug, request.Name),
            Unit = request.Unit?.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        _context.ProductSpecDefinitions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("ProductSpecDefinition", entity.Id, "Create", null, cancellationToken);
        return CreatedAtAction(nameof(GetLookups), routeValues: null, value: new SpecDefinitionDto(entity.Id, entity.Name, entity.Slug, entity.Unit, entity.SortOrder));
    }

    [HttpPost("products")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogWrite)]
    public async Task<ActionResult<AdminProductDetailsDto>> CreateProduct([FromBody] ProductUpsertRequest request, CancellationToken cancellationToken)
    {
        var product = new Product();
        try
        {
            await ApplyProductRequestAsync(product, request, isCreate: true, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("Product", product.Id, "Create", null, cancellationToken);
        return await GetProduct(product.Id, cancellationToken);
    }

    [HttpPut("products/{id:int}")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogWrite)]
    public async Task<ActionResult<AdminProductDetailsDto>> UpdateProduct(int id, [FromBody] ProductUpsertRequest request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(x => x.Variants)
            .Include(x => x.ProductTags)
            .Include(x => x.SpecValues)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        try
        {
            await ApplyProductRequestAsync(product, request, isCreate: false, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("Product", product.Id, "Update", null, cancellationToken);
        return await GetProduct(product.Id, cancellationToken);
    }

    private async Task ApplyProductRequestAsync(Product product, ProductUpsertRequest request, bool isCreate, CancellationToken cancellationToken)
    {
        var isSuperAdmin = User.IsInRole(KharbarchiRoles.SuperAdmin) || User.IsInRole(KharbarchiRoles.LegacyAdmin);
        var canEditPurchasePrice = isSuperAdmin || User.IsInRole(KharbarchiRoles.PricingManager);
        if (!canEditPurchasePrice && (request.PurchasePrice.HasValue || request.Variants.Any(x => x.PurchasePrice.HasValue)))
        {
            throw new UnauthorizedAccessException("این نقش اجازه ثبت یا ویرایش قیمت خرید را ندارد.");
        }

        // برای جلوگیری از دور زدن گردش کار، تغییر مستقیم قیمت/موجودی محصول موجود فقط برای مدیر کل مجاز است.
        // مدیر قیمت‌گذاری و کارمندها باید از فرم‌های پیشنهاد قیمت و موجودی استفاده کنند.
        if (!isCreate && !isSuperAdmin)
        {
            var directProductFinancialChange = request.Price != product.Price ||
                                               request.OldPrice != product.OldPrice ||
                                               request.PurchasePrice != product.PurchasePrice ||
                                               request.StockQuantity != product.StockQuantity;

            var directVariantFinancialChange = request.Variants.Any(incoming =>
            {
                if (!incoming.Id.HasValue)
                {
                    return incoming.Price != 0 || incoming.PurchasePrice.HasValue || incoming.StockQuantity != 0;
                }

                var current = product.Variants.FirstOrDefault(x => x.Id == incoming.Id.Value);
                return current is not null &&
                       (incoming.Price != current.Price ||
                        incoming.OldPrice != current.OldPrice ||
                        incoming.PurchasePrice != current.PurchasePrice ||
                        incoming.StockQuantity != current.StockQuantity);
            });

            if (directProductFinancialChange || directVariantFinancialChange)
            {
                throw new UnauthorizedAccessException("تغییر مستقیم قیمت یا موجودی مجاز نیست. از گردش کار پیشنهاد قیمت/موجودی استفاده کن.");
            }
        }

        if (!await _context.Categories.AnyAsync(x => x.Id == request.CategoryId, cancellationToken))
        {
            throw new InvalidOperationException("دسته‌بندی انتخاب‌شده معتبر نیست.");
        }

        product.Name = request.Name.Trim();
        product.Slug = SlugService.NormalizeSlug(request.Slug, request.Name);
        product.Sku = string.IsNullOrWhiteSpace(request.Sku) ? null : request.Sku.Trim();
        product.Description = request.Description?.Trim() ?? string.Empty;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.CommodityId = request.CommodityId;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.Price = request.Price;
        product.OldPrice = request.OldPrice;
        if (canEditPurchasePrice)
        {
            product.PurchasePrice = request.PurchasePrice;
        }
        product.StockQuantity = request.StockQuantity;
        product.MinStockAlertQuantity = request.MinStockAlertQuantity;
        product.IsAvailable = request.IsAvailable;
        product.WooCommerceProductId = request.WooCommerceProductId;
        product.UpdatedAtUtc = isCreate ? null : DateTime.UtcNow;

        product.ProductTags.Clear();
        foreach (var tagId in request.TagIds.Distinct())
        {
            product.ProductTags.Add(new ProductProductTag { ProductId = product.Id, ProductTagId = tagId });
        }

        product.SpecValues.Clear();
        foreach (var spec in request.Specs.GroupBy(x => x.SpecDefinitionId).Select(x => x.First()))
        {
            product.SpecValues.Add(new ProductSpecValue { ProductId = product.Id, SpecDefinitionId = spec.SpecDefinitionId, Value = spec.Value.Trim() });
        }

        var incomingVariantIds = request.Variants.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();
        foreach (var toRemove in product.Variants.Where(x => x.Id != 0 && !incomingVariantIds.Contains(x.Id)).ToList())
        {
            product.Variants.Remove(toRemove);
        }

        foreach (var item in request.Variants)
        {
            var variant = item.Id.HasValue ? product.Variants.FirstOrDefault(x => x.Id == item.Id.Value) : null;
            if (variant is null)
            {
                variant = new ProductVariant();
                product.Variants.Add(variant);
            }

            variant.Name = item.Name.Trim();
            variant.Sku = string.IsNullOrWhiteSpace(item.Sku) ? null : item.Sku.Trim();
            variant.Price = item.Price;
            variant.OldPrice = item.OldPrice;
            if (canEditPurchasePrice)
            {
                variant.PurchasePrice = item.PurchasePrice;
            }
            variant.StockQuantity = item.StockQuantity;
            variant.MinStockAlertQuantity = item.MinStockAlertQuantity;
            variant.IsDefault = item.IsDefault;
            variant.IsAvailable = item.IsAvailable;
            variant.WooCommerceProductId = item.WooCommerceProductId;
            variant.WooCommerceVariationId = item.WooCommerceVariationId;
        }
    }

    private async Task AuditAsync(string entityType, long entityId, string action, string? note, CancellationToken cancellationToken)
    {
        _context.ApprovalAuditLogs.Add(new ApprovalAuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserName = User.Identity?.Name ?? "unknown",
            UserRole = string.Join(",", User.Claims.Where(x => x.Type.EndsWith("role", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value)),
            Note = note
        });
        await _context.SaveChangesAsync(cancellationToken);
    }
}
