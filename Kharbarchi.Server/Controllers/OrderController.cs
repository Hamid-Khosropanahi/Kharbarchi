using Kharbarchi.Server.Data;
using Kharbarchi.Server.Security;
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
    [Authorize(Policy = AuthorizationPolicyNames.ErpOrderCreate)]
    public async Task<ActionResult<int>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        return StatusCode(StatusCodes.Status410Gone,
            "ثبت سفارش قدیمی غیرفعال شده است. سفارش باید از مسیر امن api/erp/sales/orders و برای مشتری موجود ERP ثبت شود.");
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthorizationPolicyNames.OrdersRead)]
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
