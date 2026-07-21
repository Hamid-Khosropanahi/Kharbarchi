using Kharbarchi.Server.Data;
using Kharbarchi.Server.Security;
using Kharbarchi.Server.Services;
using Kharbarchi.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicyNames.CustomersRead)]
[EnableRateLimiting("admin")]
[Route("api/erp/customers")]
public sealed class ErpCustomersController : ControllerBase
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private readonly AppDbContext _context;
    private readonly BarokCustomerImportService _importService;

    public ErpCustomersController(AppDbContext context, BarokCustomerImportService importService)
    {
        _context = context;
        _importService = importService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerListItemDto>>> Get([FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.FullName.Contains(term) || x.PhoneNumber.Contains(term) || (x.LegalEntityId != null && x.LegalEntityId.Contains(term)));
        }

        var rows = await query.OrderBy(x => x.FullName).Take(300).ToListAsync(cancellationToken);
        return Ok(rows.Select(x => new CustomerListItemDto(x.Id, x.FullName, x.PhoneNumber, x.LegalEntityId, x.AddressLine, x.City,
            x.CreditLimit, x.UsedCredit, Math.Max(0, x.CreditLimit - x.UsedCredit), x.IsCreditBlocked, x.IsActive, x.LastImportedAtUtc)).ToArray());
    }

    [HttpPost("import-barok")]
    [Authorize(Policy = AuthorizationPolicyNames.CustomerImportWrite)]
    [RequestSizeLimit(MaxFileSize * 2)]
    public async Task<ActionResult<CustomerImportResultDto>> ImportBarok(
        [FromForm] IFormFile customersFile,
        [FromForm] IFormFile creditsFile,
        CancellationToken cancellationToken)
    {
        if (customersFile is null || creditsFile is null || customersFile.Length == 0 || creditsFile.Length == 0)
            return BadRequest("هر دو فایل مشتریان و اعتبار باروک الزامی هستند.");
        if (customersFile.Length > MaxFileSize || creditsFile.Length > MaxFileSize)
            return BadRequest("حداکثر اندازه هر فایل ۱۰ مگابایت است.");

        try
        {
            await using var customers = customersFile.OpenReadStream();
            await using var credits = creditsFile.OpenReadStream();
            return Ok(await _importService.ImportAsync(customers, credits, User.Identity?.Name ?? "unknown", cancellationToken));
        }
        catch (InvalidDataException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
