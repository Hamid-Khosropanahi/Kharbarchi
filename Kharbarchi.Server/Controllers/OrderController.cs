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
            PaymentStatus = "Pending",
            Items = new List<OrderItem>()
        };

        decimal total = 0;

        foreach (var item in request.Items)
        {
            // 1. دریافت محصول و وزن انتخاب شده
            var product = await _context.Products.FindAsync(item.ProductId);
            var variant = await _context.ProductVariants.FindAsync(item.VariantId);

            if (product == null || variant == null)
                return BadRequest($"محصول یا وزن انتخاب شده نامعتبر است (ID: {item.ProductId})");

            // بررسی اینکه آیا وزن متعلق به همان محصول است
            if (variant.ProductId != product.Id)
                return BadRequest("ناهماهنگی در اطلاعات محصول و وزن.");

            // 2. ساخت آیتم سفارش با اطلاعات وزن
            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                ProductVariantId = variant.Id,
                VariantName = variant.Name, // ذخیره نام وزن برای تاریخچه
                Quantity = item.Quantity,
                UnitPrice = variant.Price // قیمت از Variant خوانده می‌شود

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
            .ThenInclude(i => i.Product) // برای گرفتن نام اصلی و تصویر محصول
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

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

                // --- تغییرات جدید ---
                ProductVariantId = i.ProductVariantId,
                VariantName = i.VariantName, // نام وزن (مثلاً "10 کیلوگرم") که در دیتابیس ذخیره شده
                                             // -------------------

                ProductName = i.Product!.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,

                // اگر در OrderItem پراپرتی LineTotal ندارید، اینجا محاسبه کنید:
                LineTotal = i.UnitPrice * i.Quantity
            }).ToList()
        };

        return Ok(dto);
    }

}