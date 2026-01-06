using Kharbarchi.Server.Data;
using Kharbarchi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var today = DateTime.UtcNow.Date;

        var totalOrders = await _context.Orders.CountAsync();
        var todayOrders = await _context.Orders.CountAsync(o => o.CreatedAt.Date == today);

        var todaySales = await _context.Orders
            .Where(o => o.CreatedAt.Date == today)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var monthSales = await _context.Orders
            .Where(o => o.CreatedAt.Month == DateTime.UtcNow.Month && o.CreatedAt.Year == DateTime.UtcNow.Year)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var users = await _context.Users.CountAsync();
        var products = await _context.Products.CountAsync();

        return Ok(new
        {
            totalOrders,
            todayOrders,
            todaySales,
            monthSales,
            users,
            products
        });
    }

    [HttpGet("latest-orders")]
    public async Task<IActionResult> GetLatestOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
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
}