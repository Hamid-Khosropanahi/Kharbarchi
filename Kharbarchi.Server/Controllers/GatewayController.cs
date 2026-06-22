using Kharbarchi.Server.Contracts;
using Kharbarchi.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Authorize(Policy = "GatewayAdminOnly")]
[EnableRateLimiting("gateway")]
[Route("api/gateway/woocommerce")]
[Produces("application/json")]
public sealed class GatewayController : ControllerBase
{
    private readonly WooCommerceSyncService _syncService;

    public GatewayController(WooCommerceSyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpPost("payment-received")]
    public async Task<ActionResult<GatewayPaymentReceivedResponse>> PaymentReceived(
        [FromBody] GatewayPaymentReceivedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userName))
        {
            return Forbid();
        }

        var result = await _syncService.RegisterGatewayPaymentAsync(request, userName, cancellationToken);
        return Ok(result);
    }
}
