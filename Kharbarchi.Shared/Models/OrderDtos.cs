using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kharbarchi.Shared.Models;

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class CustomerDto
{
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string? Email { get; set; }
    public string AddressLine { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
}

public class OrderDetailDto
{
    public int Id { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }

    public string PaymentMethod { get; set; } = default!;
    // e.g. "OnlinePayment", "CashOnDelivery", "CreditCard"

	public CustomerDto Customer { get; set; } = default!;
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderListDto
{
    public int Id { get; set; }
    public string Customer { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}