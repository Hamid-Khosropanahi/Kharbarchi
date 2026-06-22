using Kharbarchi.Shared.Contracts;
using Kharbarchi.Server.Security;
using Kharbarchi.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/admin/accounting")]
[Authorize(Policy = AuthorizationPolicyNames.AccountingOrdersRead)]
[EnableRateLimiting("payment")]
[Produces("application/json")]
public sealed class AccountingController : ControllerBase
{
    private readonly WooCommerceImportService _orders;
    private readonly AccountingReceiptService _receipts;

    public AccountingController(WooCommerceImportService orders, AccountingReceiptService receipts)
    {
        _orders = orders;
        _receipts = receipts;
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<LocalWooOrderListItemDto>>> GetOrders(
        [FromQuery] string? paymentStatus,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _orders.GetOrdersAsync(null, paymentStatus, search, page, pageSize, cancellationToken));
    }

    [HttpGet("orders/{id:long}")]
    public async Task<ActionResult<LocalWooOrderDetailsDto>> GetOrder(long id, CancellationToken cancellationToken)
    {
        var order = await _orders.GetOrderDetailsAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("orders/{id:long}/manual-receipts")]
    [Authorize(Policy = AuthorizationPolicyNames.ManualReceiptCreate)]
    public async Task<ActionResult<ManualPaymentReceiptDto>> CreateManualReceipt(long id, [FromBody] CreateManualPaymentReceiptRequest request, CancellationToken cancellationToken)
    {
        var userName = User.Identity?.Name ?? "unknown";
        var result = await _receipts.CreateManualReceiptAsync(id, request, userName, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
