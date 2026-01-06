using Kharbarchi.Server.Data;
using Kharbarchi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class OrderAdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderAdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] string? status)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        var orders = await query
            .Select(o => new
            {
                o.Id,
                Customer = o.Customer!.FullName,
                o.TotalAmount,
                o.Status,
                o.CreatedAt
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        var dto = new
        {
            order.Id,
            order.Status,
            order.CreatedAt,
            order.TotalAmount,
            Customer = new
            {
                order.Customer!.FullName,
                order.Customer.PhoneNumber,
                order.Customer.Email,
                order.Customer.AddressLine,
                order.Customer.City,
                order.Customer.PostalCode
            },
            Items = order.Items.Select(i => new
            {
                i.ProductId,
                ProductName = i.Product!.Name,
                i.Quantity,
                i.UnitPrice,
                i.LineTotal
            })
        };

        return Ok(dto);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = newStatus;
        await _context.SaveChangesAsync();
        return Ok();
    }
}