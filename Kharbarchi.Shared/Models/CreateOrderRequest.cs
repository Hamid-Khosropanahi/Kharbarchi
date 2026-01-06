using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kharbarchi.Shared.Models;

public class CreateOrderRequest
{
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string? Email { get; set; }

    public string AddressLine { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;

    public string PaymentMethod { get; set; } = "Offline";

    public List<CartItemDto> Items { get; set; } = new();
}