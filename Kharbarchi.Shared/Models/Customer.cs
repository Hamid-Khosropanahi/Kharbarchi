namespace Kharbarchi.Shared.Models;

public class Customer
{
    public int Id { get; set; }

    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string? Email { get; set; }

    public string AddressLine { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}