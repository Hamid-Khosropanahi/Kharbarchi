using System.Data;
using Kharbarchi.Server.Data;
using Kharbarchi.Server.Security;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Authorize]
[EnableRateLimiting("admin")]
[Route("api/erp/sales")]
public sealed class ErpSalesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ErpSalesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("catalog")]
    [Authorize(Policy = AuthorizationPolicyNames.SellerCatalogRead)]
    public async Task<ActionResult<IReadOnlyList<ErpCatalogItemDto>>> GetCatalog([FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(x => x.Brand)
            .Include(x => x.Variants)
            .Where(x => x.IsAvailable && x.Price > 0);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || (x.Sku != null && x.Sku.Contains(term)));
        }

        var products = await query.OrderBy(x => x.Name).Take(300).ToListAsync(cancellationToken);
        var rows = new List<ErpCatalogItemDto>();
        foreach (var product in products)
        {
            var variants = product.Variants.Where(x => x.IsAvailable && x.Price > 0).OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name).ToArray();
            if (variants.Length == 0)
            {
                rows.Add(new ErpCatalogItemDto(product.Id, null, product.Name, null, product.Sku, product.Price, product.OldPrice,
                    product.StockQuantity, product.Brand?.Name, product.ImageUrl));
                continue;
            }

            rows.AddRange(variants.Select(variant => new ErpCatalogItemDto(product.Id, variant.Id, product.Name, variant.Name,
                variant.Sku ?? product.Sku, variant.Price, variant.OldPrice, variant.StockQuantity, product.Brand?.Name, product.ImageUrl)));
        }
        return Ok(rows);
    }

    [HttpPost("orders")]
    [Authorize(Policy = AuthorizationPolicyNames.ErpOrderCreate)]
    public async Task<ActionResult<ErpOrderCreatedDto>> CreateOrder([FromBody] ErpOrderCreateRequest request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
            return BadRequest("سبد سفارش خالی است.");
        if (request.TotalDiscount < 0)
            return BadRequest("تخفیف نمی‌تواند منفی باشد.");

        var paymentMethod = request.PaymentMethod.Trim();
        if (!paymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase) && !paymentMethod.Equals("Credit", StringComparison.OrdinalIgnoreCase))
            return BadRequest("روش پرداخت باید Cash یا Credit باشد.");

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == request.CustomerId && x.IsActive, cancellationToken);
        if (customer is null)
            return BadRequest("مشتری انتخاب‌شده فعال یا موجود نیست.");

        var order = new Order
        {
            CustomerId = customer.Id,
            Status = "New",
            PaymentMethod = paymentMethod,
            PaymentStatus = "Pending",
            DeliveryAddressLine = request.DeliveryAddressLine.Trim(),
            DeliveryCity = request.DeliveryCity.Trim(),
            DeliveryPostalCode = request.DeliveryPostalCode?.Trim() ?? string.Empty,
            Description = request.Description?.Trim(),
            CreatedByUserName = User.Identity?.Name ?? "unknown",
            Items = []
        };

        decimal gross = 0;
        foreach (var requestedLine in request.Items)
        {
            if (requestedLine.Quantity <= 0)
                return BadRequest("تعداد همه اقلام باید بیشتر از صفر باشد.");

            var product = await _context.Products
                .Include(x => x.Variants)
                .FirstOrDefaultAsync(x => x.Id == requestedLine.ProductId && x.IsAvailable, cancellationToken);
            if (product is null)
                return BadRequest($"محصول {requestedLine.ProductId} فعال یا موجود نیست.");

            ProductVariant? variant = null;
            if (requestedLine.ProductVariantId.HasValue)
            {
                variant = product.Variants.FirstOrDefault(x => x.Id == requestedLine.ProductVariantId.Value && x.IsAvailable);
                if (variant is null)
                    return BadRequest($"حالت انتخاب‌شده برای محصول «{product.Name}» معتبر نیست.");
            }
            else if (product.Variants.Any(x => x.IsAvailable))
            {
                return BadRequest($"برای محصول «{product.Name}» باید نوع بسته‌بندی/حالت فروش انتخاب شود.");
            }

            var price = variant?.Price ?? product.Price;
            var stock = variant?.StockQuantity ?? product.StockQuantity;
            if (price <= 0)
                return BadRequest($"محصول «{product.Name}» قیمت فعال ندارد.");
            if (stock < requestedLine.Quantity)
                return BadRequest($"موجودی «{product.Name}{(variant is null ? string.Empty : " - " + variant.Name)}» کافی نیست.");

            if (variant is null) product.StockQuantity -= requestedLine.Quantity;
            else variant.StockQuantity -= requestedLine.Quantity;

            var lineTotal = price * requestedLine.Quantity;
            gross += lineTotal;
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductVariantId = variant?.Id,
                ProductName = product.Name,
                VariantName = variant?.Name,
                Sku = variant?.Sku ?? product.Sku,
                WooCommerceProductId = variant?.WooCommerceProductId ?? product.WooCommerceProductId,
                WooCommerceVariationId = variant?.WooCommerceVariationId,
                Quantity = requestedLine.Quantity,
                OriginalUnitPrice = price,
                UnitPrice = price,
                LineDiscount = 0
            });
        }

        if (request.TotalDiscount > gross)
            return BadRequest("تخفیف کل نمی‌تواند از جمع ناخالص سفارش بیشتر باشد.");

        var total = gross - request.TotalDiscount;
        if (paymentMethod.Equals("Credit", StringComparison.OrdinalIgnoreCase))
        {
            if (customer.IsCreditBlocked)
                return Conflict("اعتبار این مشتری در باروک مسدود است.");
            if (customer.CreditLimit - customer.UsedCredit < total)
                return Conflict("اعتبار باقیمانده مشتری برای این سفارش کافی نیست.");
            customer.UsedCredit += total;
        }

        order.GrossAmount = gross;
        order.TotalDiscount = request.TotalDiscount;
        order.TotalAmount = total;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new ErpOrderCreatedDto(order.Id, gross, request.TotalDiscount, total));
    }
}
