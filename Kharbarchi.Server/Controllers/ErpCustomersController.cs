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
    private const long MaxFileSize = 100 * 1024 * 1024;
    private readonly AppDbContext _context;
    private readonly BarokCustomerImportService _importService;

    public ErpCustomersController(AppDbContext context, BarokCustomerImportService importService)
    {
        _context = context;
        _importService = importService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedCustomerResultDto>> Get(
        [FromQuery] string? search = null,
        [FromQuery] string? customerType = null,
        [FromQuery] bool? isBlocked = null,
        [FromQuery] string? province = null,
        [FromQuery] string? city = null,
        [FromQuery] decimal? minCreditLimit = null,
        [FromQuery] decimal? maxCreditLimit = null,
        [FromQuery] decimal? minAvailableCredit = null,
        [FromQuery] decimal? maxAvailableCredit = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = new[] { 5, 10, 20, 50, 100 }.Contains(pageSize) ? pageSize : 20;
        var query = _context.Customers.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.FullName.Contains(term) || x.PhoneNumber.Contains(term)
                || (x.FirstName != null && x.FirstName.Contains(term)) || (x.LastName != null && x.LastName.Contains(term))
                || (x.StoreName != null && x.StoreName.Contains(term)) || (x.NationalCode != null && x.NationalCode.Contains(term))
                || (x.LegalEntityId != null && x.LegalEntityId.Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(customerType)) query = query.Where(x => x.CustomerType == customerType);
        if (isBlocked.HasValue) query = query.Where(x => x.IsCreditBlocked == isBlocked.Value);
        if (!string.IsNullOrWhiteSpace(province)) query = query.Where(x => x.Province != null && x.Province.Contains(province.Trim()));
        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(x => x.City.Contains(city.Trim()));
        if (minCreditLimit.HasValue) query = query.Where(x => x.CreditLimit >= minCreditLimit.Value);
        if (maxCreditLimit.HasValue) query = query.Where(x => x.CreditLimit <= maxCreditLimit.Value);
        if (minAvailableCredit.HasValue) query = query.Where(x => x.CreditLimit - x.UsedCredit >= minAvailableCredit.Value);
        if (maxAvailableCredit.HasValue) query = query.Where(x => x.CreditLimit - x.UsedCredit <= maxAvailableCredit.Value);

        var total = await query.LongCountAsync(cancellationToken);
        var rows = await query.OrderBy(x => x.FullName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = rows.Select(x => new CustomerListItemDto(x.Id, x.FullName, x.PhoneNumber, x.LegalEntityId, x.NationalCode,
            x.CustomerType, x.StoreName, x.Province, x.AddressLine, x.City, x.CreditLimit, x.UsedCredit,
            Math.Max(0, x.CreditLimit - x.UsedCredit), x.IsCreditBlocked, x.IsActive, x.LastImportedAtUtc)).ToArray();
        return Ok(new PagedCustomerResultDto(items, page, pageSize, total, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize))));
    }

    [HttpPost("import-barok")]
    [Authorize(Policy = AuthorizationPolicyNames.CustomerImportWrite)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize * 2)]
    public async Task<ActionResult<CustomerImportResultDto>> ImportBarok(
        IFormFile customersFile,
        IFormFile creditsFile,
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

    [HttpPost("import-details")]
    [Authorize(Policy = AuthorizationPolicyNames.CustomerImportWrite)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<ActionResult<CustomerImportResultDto>> ImportDetails(IFormFile customersFile, [FromQuery] string customerType = CustomerTypes.Legal, CancellationToken cancellationToken = default)
    {
        if (customersFile is null || customersFile.Length == 0)
            return BadRequest("فایل اطلاعات مشتریان الزامی است.");
        if (customersFile.Length > MaxFileSize)
            return BadRequest("حداکثر اندازه فایل ۱۰۰ مگابایت است.");
        try
        {
            await using var stream = customersFile.OpenReadStream();
            if (customerType is not CustomerTypes.Legal and not CustomerTypes.Individual) return BadRequest("نوع مشتری معتبر نیست.");
            return Ok(await _importService.ImportCustomerDetailsAsync(stream, customerType, User.Identity?.Name ?? "unknown", cancellationToken));
        }
        catch (InvalidDataException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("import-credits")]
    [Authorize(Policy = AuthorizationPolicyNames.CustomerImportWrite)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<ActionResult<CustomerImportResultDto>> ImportCredits(IFormFile creditsFile, [FromQuery] string customerType = CustomerTypes.Legal, CancellationToken cancellationToken = default)
    {
        if (creditsFile is null || creditsFile.Length == 0)
            return BadRequest("فایل اعتبار مشتریان الزامی است.");
        if (creditsFile.Length > MaxFileSize)
            return BadRequest("حداکثر اندازه فایل ۱۰۰ مگابایت است.");
        try
        {
            await using var stream = creditsFile.OpenReadStream();
            if (customerType is not CustomerTypes.Legal and not CustomerTypes.Individual) return BadRequest("نوع مشتری معتبر نیست.");
            return Ok(await _importService.ImportCreditsAsync(stream, customerType, User.Identity?.Name ?? "unknown", cancellationToken));
        }
        catch (InvalidDataException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
