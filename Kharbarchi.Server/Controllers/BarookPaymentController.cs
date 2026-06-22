using Kharbarchi.Shared.Contracts;
using Kharbarchi.Server.Security;
using Kharbarchi.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Route("api/admin/barook-payments")]
[Authorize(Policy = AuthorizationPolicyNames.BarookPaymentOperator)]
[EnableRateLimiting("payment")]
[Produces("application/json")]
public sealed class BarookPaymentController : ControllerBase
{
    private readonly BarookPaymentService _barookPaymentService;

    public BarookPaymentController(BarookPaymentService barookPaymentService)
    {
        _barookPaymentService = barookPaymentService;
    }

    [HttpPost("orders/{orderId:long}/start")]
    public async Task<ActionResult<StartBarookPaymentResponse>> StartPayment(long orderId, [FromBody] StartBarookPaymentRequest request, CancellationToken cancellationToken)
    {
        var userName = User.Identity?.Name ?? "unknown";
        var result = await _barookPaymentService.StartPaymentAsync(orderId, request, userName, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("sessions/{sessionId:long}/mark-link-sent")]
    public async Task<IActionResult> MarkLinkSent(long sessionId, [FromBody] MarkBarookPaymentLinkSentRequest request, CancellationToken cancellationToken)
    {
        var userName = User.Identity?.Name ?? "unknown";
        var ok = await _barookPaymentService.MarkLinkSentAsync(sessionId, request, userName, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("sessions/{sessionId:long}/verify")]
    public async Task<ActionResult<VerifyBarookPaymentResponse>> VerifyPayment(long sessionId, [FromBody] VerifyBarookPaymentRequest request, CancellationToken cancellationToken)
    {
        var userName = User.Identity?.Name ?? "unknown";
        var result = await _barookPaymentService.VerifyPaymentAsync(sessionId, request, userName, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
