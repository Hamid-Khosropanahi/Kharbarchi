using Kharbarchi.Shared.Contracts;
using Kharbarchi.Server.Security;
using Kharbarchi.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/admin/local-orders")]
[Authorize(Policy = AuthorizationPolicyNames.OrdersRead)]
[EnableRateLimiting("admin")]
[Produces("application/json")]
public sealed class LocalOrdersController : ControllerBase
{
    private readonly WooCommerceImportService _orders;

    public LocalOrdersController(WooCommerceImportService orders)
    {
        _orders = orders;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LocalWooOrderListItemDto>>> GetOrders(
        [FromQuery] string? internalStatus,
        [FromQuery] string? paymentStatus,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _orders.GetOrdersAsync(internalStatus, paymentStatus, search, page, pageSize, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<LocalWooOrderDetailsDto>> GetOrder(long id, CancellationToken cancellationToken)
    {
        var order = await _orders.GetOrderDetailsAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:long}/internal-status")]
    [Authorize(Policy = AuthorizationPolicyNames.OrderPaymentWorkflow)]
    public async Task<IActionResult> ChangeInternalStatus(long id, [FromBody] ChangeInternalOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var userName = User.Identity?.Name ?? "unknown";
        var ok = await _orders.ChangeInternalStatusAsync(id, request, userName, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
