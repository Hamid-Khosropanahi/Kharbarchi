using System.Text.Json;
using Kharbarchi.Server.Contracts;
using Kharbarchi.Server.Services;
using Kharbarchi.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicyNames.CentralSyncAgentOnly)]
[EnableRateLimiting("admin")]
[Route("api/admin/woocommerce")]
[Produces("application/json")]
public sealed class WooCommerceSupportController : ControllerBase
{
    private readonly WooCommerceSyncService _syncService;

    public WooCommerceSupportController(WooCommerceSyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpGet("local-products")]
    public async Task<ActionResult<IReadOnlyList<LocalProductSyncDto>>> GetLocalProducts(CancellationToken cancellationToken = default)
    {
        var rows = await _syncService.GetLocalProductsAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpPost("sync-products")]
    public async Task<ActionResult<SyncProductsResult>> SyncProducts([FromBody] SyncProductsRequest request, CancellationToken cancellationToken = default)
    {
        var userName = User.Identity?.Name ?? "unknown-admin";
        var result = await _syncService.SyncProductsAsync(request.ProductIds, request.DryRun, userName, cancellationToken);
        return Ok(result);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetWooCommerceOrders(
        [FromQuery] string? status = "processing",
        [FromQuery] DateTime? afterUtc = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        using var orders = await _syncService.GetWooCommerceOrdersAsync(status, afterUtc, page, pageSize, cancellationToken);
        return Content(orders.RootElement.GetRawText(), "application/json");
    }
}
