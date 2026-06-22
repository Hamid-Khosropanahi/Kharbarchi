using Kharbarchi.Server.Data;
using Kharbarchi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class OrderController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<int>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest("سبد خرید خالی است.");
        }

        if (request.Items.Any(i => i.Quantity <= 0))
        {
            return BadRequest("تعداد کالا باید بزرگ‌تر از صفر باشد.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.PhoneNumber) ||
            string.IsNullOrWhiteSpace(request.AddressLine) ||
            string.IsNullOrWhiteSpace(request.City))
        {
            return BadRequest("نام، شماره تماس، آدرس و شهر الزامی هستند.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var customer = new Customer
        {
            FullName = (request.FullName ?? string.Empty).Trim(),
            PhoneNumber = (request.PhoneNumber ?? string.Empty).Trim(),
            Email = request.Email?.Trim(),
            AddressLine = (request.AddressLine ?? string.Empty).Trim(),
            City = (request.City ?? string.Empty).Trim(),
            PostalCode = string.IsNullOrWhiteSpace(request.PostalCode) ? string.Empty : request.PostalCode.Trim()
        };

        var order = new Order
        {
            Customer = customer,
            Status = "New",
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Offline" : request.PaymentMethod.Trim(),
            PaymentStatus = "Pending",
            Items = new List<OrderItem>()
        };

        decimal total = 0;

        foreach (var item in request.Items)
        {
            if (item.VariantId > 0)
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.Id == item.VariantId && v.ProductId == item.ProductId, cancellationToken);

                if (variant is null || variant.Product is null || !variant.Product.IsAvailable)
                {
                    return BadRequest($"محصول یا حالت انتخاب‌شده نامعتبر است. ProductId={item.ProductId}, VariantId={item.VariantId}");
                }

                if (variant.StockQuantity < item.Quantity)
                {
                    return BadRequest($"موجودی «{variant.Product.Name} - {variant.Name}» کافی نیست.");
                }

                variant.StockQuantity -= item.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = variant.ProductId,
                    ProductVariantId = variant.Id,
                    VariantName = variant.Name,
                    Sku = variant.Sku ?? variant.Product.Sku,
                    WooCommerceProductId = variant.WooCommerceProductId ?? variant.Product.WooCommerceProductId,
                    WooCommerceVariationId = variant.WooCommerceVariationId,
                    Quantity = item.Quantity,
                    UnitPrice = variant.Price
                };

                total += orderItem.UnitPrice * orderItem.Quantity;
                order.Items.Add(orderItem);
            }
            else
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);
                if (product is null || !product.IsAvailable)
                {
                    return BadRequest($"محصول انتخاب‌شده نامعتبر است. ProductId={item.ProductId}");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    return BadRequest($"موجودی «{product.Name}» کافی نیست.");
                }

                product.StockQuantity -= item.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductVariantId = null,
                    VariantName = null,
                    Sku = product.Sku,
                    WooCommerceProductId = product.WooCommerceProductId,
                    WooCommerceVariationId = null,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                total += orderItem.UnitPrice * orderItem.Quantity;
                order.Items.Add(orderItem);
            }
        }

        order.TotalAmount = total;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(order.Id);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<OrderDetailDto>> GetOrder(int id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        var dto = new OrderDetailDto
        {
            Id = order.Id,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod,
            Customer = new CustomerDto
            {
                FullName = order.Customer!.FullName,
                PhoneNumber = order.Customer.PhoneNumber,
                Email = order.Customer.Email,
                AddressLine = order.Customer.AddressLine,
                City = order.Customer.City,
                PostalCode = order.Customer.PostalCode
            },
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductVariantId = i.ProductVariantId,
                VariantName = i.VariantName,
                ProductName = i.Product!.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.UnitPrice * i.Quantity
            }).ToList()
        };

        return Ok(dto);
    }
}
