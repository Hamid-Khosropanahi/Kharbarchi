using Kharbarchi.Server.Data;
using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (request.Items == null || !request.Items.Any())
            return BadRequest("سبد خرید خالی است.");

        var customer = new Customer
        {
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            AddressLine = request.AddressLine,
            City = request.City,
            PostalCode = request.PostalCode
        };

        var order = new Order
        {
            Customer = customer,
            Status = "New",
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = "Pending"
        };

        decimal total = 0;

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
            if (product == null)
                return BadRequest($"محصول با شناسه {item.ProductId} یافت نشد.");

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            total += orderItem.UnitPrice * orderItem.Quantity;
            order.Items.Add(orderItem);
        }

        order.TotalAmount = total;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(order.Id);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailDto>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        var dto = new OrderDetailDto
        {
            Id = order.Id,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod, // map from domain entity
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
                ProductName = i.Product!.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal
            }).ToList()
        };

        return Ok(dto);
    }
}