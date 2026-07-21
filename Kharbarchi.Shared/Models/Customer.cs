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

    // شناسه یکتای شرکت، کلید تطبیق پایدار فایل‌های باروک است.
    public string? LegalEntityId { get; set; }
    public bool IsLegalEntity { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCreditBlocked { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal UsedCredit { get; set; }
    public string? CreditPlanTitle { get; set; }
    public string? DistributionStatus { get; set; }
    public DateTime? CreditReceivedAtUtc { get; set; }
    public DateTime? LastImportedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public decimal AvailableCredit => Math.Max(0, CreditLimit - UsedCredit);

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CustomerCreditHistory> CreditHistory { get; set; } = new List<CustomerCreditHistory>();
}
