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
    public string CustomerType { get; set; } = "Legal";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? NationalCode { get; set; }
    public string? StoreName { get; set; }
    public string? Province { get; set; }
    public string? BusinessCategory { get; set; }
    public bool IsLegalEntity { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCreditBlocked { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal UsedCredit { get; set; }
    public string? CreditPlanTitle { get; set; }
    public string? DistributionStatus { get; set; }
    public bool IsDefinedByDistribution { get; set; }
    public DateTime? CreditReceivedAtUtc { get; set; }
    public DateTime? CreditExpiresAtUtc { get; set; }
    public decimal? SourceRemainingCredit { get; set; }
    public decimal? AllowedSpending { get; set; }
    public DateTime? LastImportedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public decimal AvailableCredit => Math.Max(0, CreditLimit - UsedCredit);

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CustomerCreditHistory> CreditHistory { get; set; } = new List<CustomerCreditHistory>();
}
