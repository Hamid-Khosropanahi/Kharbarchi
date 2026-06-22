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
[Authorize(Policy = AuthorizationPolicyNames.StockRead)]
[EnableRateLimiting("admin")]
[Route("api/admin/inventory")]
[Produces("application/json")]
public sealed class InventoryWorkflowController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly SyncOutboxService _outboxService;

    public InventoryWorkflowController(AppDbContext context, SyncOutboxService outboxService)
    {
        _context = context;
        _outboxService = outboxService;
    }

    [HttpGet("proposals")]
    public async Task<ActionResult<IReadOnlyList<InventoryProposalDto>>> GetProposals([FromQuery] WorkflowStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.InventoryProposals
            .AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.ProductVariant)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var rows = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(200)
            .Select(x => new InventoryProposalDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                ProductName = x.Product!.Name,
                VariantName = x.ProductVariant != null ? x.ProductVariant.Name : null,
                Sku = x.ProductVariant != null ? x.ProductVariant.Sku : x.Product.Sku,
                CurrentStockQuantity = x.CurrentStockQuantity,
                ProposedQuantity = x.ProposedQuantity,
                FinalStockQuantity = x.FinalStockQuantity,
                AdjustmentKind = x.AdjustmentKind,
                Status = x.Status,
                CreatedByUserName = x.CreatedByUserName,
                ManagerApprovedByUserName = x.ManagerApprovedByUserName,
                SuperAdminApprovedByUserName = x.SuperAdminApprovedByUserName,
                Reason = x.Reason,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpPost("proposals")]
    [Authorize(Policy = AuthorizationPolicyNames.InventoryProposalCreate)]
    public async Task<ActionResult<InventoryProposalDto>> CreateProposal([FromBody] InventoryProposalCreateRequest request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            return NotFound("محصول پیدا نشد.");
        }

        ProductVariant? variant = null;
        if (request.ProductVariantId.HasValue)
        {
            variant = product.Variants.FirstOrDefault(x => x.Id == request.ProductVariantId.Value);
            if (variant is null)
            {
                return NotFound("حالت فروش محصول پیدا نشد.");
            }
        }

        var currentStock = variant?.StockQuantity ?? product.StockQuantity;
        var finalStock = CalculateFinalStock(currentStock, request.AdjustmentKind, request.ProposedQuantity);

        var proposal = new InventoryProposal
        {
            ProductId = product.Id,
            ProductVariantId = variant?.Id,
            CurrentStockQuantity = currentStock,
            ProposedQuantity = request.ProposedQuantity,
            FinalStockQuantity = finalStock,
            AdjustmentKind = request.AdjustmentKind,
            CreatedByUserName = User.Identity?.Name ?? "unknown",
            Reason = request.Reason?.Trim(),
            Status = WorkflowStatus.Submitted
        };

        _context.InventoryProposals.Add(proposal);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("InventoryProposal", proposal.Id, "Submit", proposal.Reason, cancellationToken);
        return CreatedAtAction(nameof(GetProposals), routeValues: new { id = proposal.Id }, value: await MapAsync(proposal.Id, cancellationToken));
    }

    [HttpPost("proposals/{id:long}/approve-manager")]
    [Authorize(Policy = AuthorizationPolicyNames.InventoryProposalManagerApproval)]
    public async Task<ActionResult<InventoryProposalDto>> ApproveByManager(long id, [FromBody] ApprovalRequest request, CancellationToken cancellationToken)
    {
        var proposal = await _context.InventoryProposals.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (proposal is null)
        {
            return NotFound();
        }

        if (proposal.Status != WorkflowStatus.Submitted)
        {
            return Conflict("این پیشنهاد در وضعیت قابل تایید مدیر قیمت‌گذاری نیست.");
        }

        proposal.Status = WorkflowStatus.ManagerApproved;
        proposal.ManagerApprovedByUserName = User.Identity?.Name ?? "unknown";
        proposal.ManagerApprovedAtUtc = DateTime.UtcNow;
        proposal.ManagerNote = request.Note?.Trim();
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("InventoryProposal", proposal.Id, "ManagerApprove", request.Note, cancellationToken);
        return Ok(await MapAsync(proposal.Id, cancellationToken));
    }

    [HttpPost("proposals/{id:long}/approve-superadmin")]
    [Authorize(Policy = AuthorizationPolicyNames.InventoryProposalFinalApproval)]
    public async Task<ActionResult<InventoryProposalDto>> ApproveBySuperAdmin(long id, [FromBody] ApprovalRequest request, CancellationToken cancellationToken)
    {
        var proposal = await _context.InventoryProposals
            .Include(x => x.Product)
            .Include(x => x.ProductVariant)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (proposal is null)
        {
            return NotFound();
        }

        if (proposal.Status is not WorkflowStatus.ManagerApproved and not WorkflowStatus.Submitted)
        {
            return Conflict("این پیشنهاد در وضعیت قابل تایید مدیر کل نیست.");
        }

        if (proposal.ProductVariant is not null)
        {
            proposal.ProductVariant.StockQuantity = Math.Max(0, proposal.FinalStockQuantity);
        }
        else
        {
            proposal.Product!.StockQuantity = Math.Max(0, proposal.FinalStockQuantity);
        }

        proposal.Status = WorkflowStatus.QueuedForSync;
        proposal.SuperAdminApprovedByUserName = User.Identity?.Name ?? "unknown";
        proposal.SuperAdminApprovedAtUtc = DateTime.UtcNow;
        proposal.SuperAdminNote = request.Note?.Trim();
        proposal.QueuedForSyncAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _outboxService.QueueProductForCentralSyncAsync(proposal.ProductId, proposal.ProductVariantId, "InventoryProposal", User.Identity?.Name ?? "unknown", cancellationToken);
        await AuditAsync("InventoryProposal", proposal.Id, "SuperAdminApproveAndQueue", request.Note, cancellationToken);
        return Ok(await MapAsync(proposal.Id, cancellationToken));
    }

    [HttpPost("proposals/{id:long}/reject")]
    [Authorize(Policy = AuthorizationPolicyNames.InventoryProposalManagerApproval)]
    public async Task<ActionResult<InventoryProposalDto>> Reject(long id, [FromBody] RejectRequest request, CancellationToken cancellationToken)
    {
        var proposal = await _context.InventoryProposals.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (proposal is null)
        {
            return NotFound();
        }

        if (proposal.Status is WorkflowStatus.Synced or WorkflowStatus.QueuedForSync)
        {
            return Conflict("پیشنهاد ارسال‌شده قابل رد کردن نیست.");
        }

        proposal.Status = WorkflowStatus.Rejected;
        proposal.RejectedByUserName = User.Identity?.Name ?? "unknown";
        proposal.RejectedAtUtc = DateTime.UtcNow;
        proposal.RejectionReason = request.Reason.Trim();
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("InventoryProposal", proposal.Id, "Reject", request.Reason, cancellationToken);
        return Ok(await MapAsync(proposal.Id, cancellationToken));
    }

    private static int CalculateFinalStock(int currentStock, InventoryAdjustmentKind kind, int proposedQuantity)
    {
        return kind switch
        {
            InventoryAdjustmentKind.SetAbsoluteStock => proposedQuantity,
            InventoryAdjustmentKind.IncreaseStock => checked(currentStock + proposedQuantity),
            InventoryAdjustmentKind.DecreaseStock => Math.Max(0, currentStock - proposedQuantity),
            InventoryAdjustmentKind.Shortage => Math.Max(0, currentStock - proposedQuantity),
            InventoryAdjustmentKind.Excess => checked(currentStock + proposedQuantity),
            _ => proposedQuantity
        };
    }

    private async Task<InventoryProposalDto> MapAsync(long id, CancellationToken cancellationToken)
    {
        return await _context.InventoryProposals
            .AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.ProductVariant)
            .Where(x => x.Id == id)
            .Select(x => new InventoryProposalDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                ProductName = x.Product!.Name,
                VariantName = x.ProductVariant != null ? x.ProductVariant.Name : null,
                Sku = x.ProductVariant != null ? x.ProductVariant.Sku : x.Product.Sku,
                CurrentStockQuantity = x.CurrentStockQuantity,
                ProposedQuantity = x.ProposedQuantity,
                FinalStockQuantity = x.FinalStockQuantity,
                AdjustmentKind = x.AdjustmentKind,
                Status = x.Status,
                CreatedByUserName = x.CreatedByUserName,
                ManagerApprovedByUserName = x.ManagerApprovedByUserName,
                SuperAdminApprovedByUserName = x.SuperAdminApprovedByUserName,
                Reason = x.Reason,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .SingleAsync(cancellationToken);
    }

    private async Task AuditAsync(string entityType, long entityId, string action, string? note, CancellationToken cancellationToken)
    {
        _context.ApprovalAuditLogs.Add(new ApprovalAuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserName = User.Identity?.Name ?? "unknown",
            UserRole = string.Join(",", User.Claims.Where(x => x.Type.EndsWith("role", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value)),
            Note = note
        });
        await _context.SaveChangesAsync(cancellationToken);
    }
}
