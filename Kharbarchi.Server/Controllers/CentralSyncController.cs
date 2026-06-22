using Kharbarchi.Server.Data;
using Kharbarchi.Server.Security;
using Kharbarchi.Server.Services;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicyNames.CentralSyncAgentOnly)]
[EnableRateLimiting("admin")]
[Route("api/central-sync/outbox")]
[Produces("application/json")]
public sealed class CentralSyncController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly SyncOutboxService _outboxService;

    public CentralSyncController(AppDbContext context, SyncOutboxService outboxService)
    {
        _context = context;
        _outboxService = outboxService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SyncOutboxDto>>> GetOutbox([FromQuery] OutboxStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SyncOutboxMessages.AsNoTracking().AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var rows = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(300)
            .Select(x => new SyncOutboxDto(x.Id, x.EventType, x.AggregateType, x.AggregateId, x.Status, x.SourceWorkflow, x.QueuedByUserName, x.CreatedAtUtc, x.RetryCount, x.LastError))
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    // سیستم مرکزی بعد از گرفتن JWT از سرور لوکال، فقط از این API پیام‌های تایید نهایی را می‌گیرد.
    [HttpPost("claim")]
    public async Task<ActionResult<IReadOnlyList<SyncOutboxPayloadDto>>> ClaimPending([FromQuery] int take = 25, CancellationToken cancellationToken = default)
    {
        var lockedBy = User.Identity?.Name ?? "central-sync-agent";
        var messages = await _outboxService.GetPendingForCentralAgentAsync(take, lockedBy, cancellationToken);
        return Ok(messages.Select(x => new SyncOutboxPayloadDto(x.Id, x.EventType, x.PayloadJson)).ToList());
    }

    [HttpPost("{id:long}/mark-sent")]
    public async Task<IActionResult> MarkSent(long id, [FromBody] MarkOutboxSentRequest request, CancellationToken cancellationToken)
    {
        await _outboxService.MarkSentAsync(id, User.Identity?.Name ?? "central-sync-agent", request.Note, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:long}/mark-failed")]
    public async Task<IActionResult> MarkFailed(long id, [FromBody] MarkOutboxFailedRequest request, CancellationToken cancellationToken)
    {
        await _outboxService.MarkFailedAsync(id, User.Identity?.Name ?? "central-sync-agent", request.Error, cancellationToken);
        return NoContent();
    }
}
