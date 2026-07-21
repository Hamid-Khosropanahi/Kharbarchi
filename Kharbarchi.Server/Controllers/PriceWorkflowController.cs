using Kharbarchi.Server.Data;
using Kharbarchi.Server.Security;
using Kharbarchi.Server.Services;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Shared.Models;
using Kharbarchi.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicyNames.PriceRead)]
[EnableRateLimiting("admin")]
[Route("api/admin/pricing")]
[Produces("application/json")]
public sealed class PriceWorkflowController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly SyncOutboxService _outboxService;

    public PriceWorkflowController(AppDbContext context, SyncOutboxService outboxService)
    {
        _context = context;
        _outboxService = outboxService;
    }

    [HttpGet("proposals")]
    public async Task<ActionResult<IReadOnlyList<PriceProposalDto>>> GetProposals([FromQuery] WorkflowStatus? status = null, CancellationToken cancellationToken = default)
    {
        var canSeePurchasePrice = CanSeePurchasePrice();
        var query = _context.PriceProposals
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
            .Select(x => new PriceProposalDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                ProductName = x.Product!.Name,
                VariantName = x.ProductVariant != null ? x.ProductVariant.Name : null,
                Sku = x.ProductVariant != null ? x.ProductVariant.Sku : x.Product.Sku,
                CurrentSalePrice = x.CurrentSalePrice,
                ProposedSalePrice = x.ProposedSalePrice,
                CurrentPurchasePrice = canSeePurchasePrice ? x.CurrentPurchasePrice : null,
                ProposedPurchasePrice = canSeePurchasePrice ? x.ProposedPurchasePrice : null,
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
    [Authorize(Policy = AuthorizationPolicyNames.PriceProposalCreate)]
    public async Task<ActionResult<PriceProposalDto>> CreateProposal([FromBody] PriceProposalCreateRequest request, CancellationToken cancellationToken)
    {
        if (!CanSeePurchasePrice() && request.ProposedPurchasePrice.HasValue)
        {
            return Forbid();
        }

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

        var proposal = new PriceProposal
        {
            ProductId = product.Id,
            ProductVariantId = variant?.Id,
            CurrentSalePrice = variant?.Price ?? product.Price,
            ProposedSalePrice = request.ProposedSalePrice,
            CurrentPurchasePrice = variant?.PurchasePrice ?? product.PurchasePrice,
            ProposedPurchasePrice = request.ProposedPurchasePrice,
            CreatedByUserName = User.Identity?.Name ?? "unknown",
            Reason = request.Reason?.Trim(),
            Status = WorkflowStatus.Submitted
        };

        _context.PriceProposals.Add(proposal);
        await _context.SaveChangesAsync(cancellationToken);
        await AuditAsync("PriceProposal", proposal.Id, "Submit", proposal.Reason, cancellationToken);
        return CreatedAtAction(nameof(GetProposals), routeValues: new { id = proposal.Id }, value: await MapAsync(proposal.Id, cancellationToken));
    }

    [HttpPost("proposals/{id:long}/approve-manager")]
    [Authorize(Policy = AuthorizationPolicyNames.PriceProposalManagerApproval)]
    public async Task<ActionResult<PriceProposalDto>> ApproveByManager(long id, [FromBody] ApprovalRequest request, CancellationToken cancellationToken)
    {
        var proposal = await _context.PriceProposals.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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
        await AuditAsync("PriceProposal", proposal.Id, "ManagerApprove", request.Note, cancellationToken);
        return Ok(await MapAsync(proposal.Id, cancellationToken));
    }

    [HttpPost("proposals/{id:long}/approve-superadmin")]
    [Authorize(Policy = AuthorizationPolicyNames.PriceProposalFinalApproval)]
    public async Task<ActionResult<PriceProposalDto>> ApproveBySuperAdmin(long id, [FromBody] ApprovalRequest request, CancellationToken cancellationToken)
    {
        var proposal = await _context.PriceProposals
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
            proposal.ProductVariant.Price = proposal.ProposedSalePrice;
            if (proposal.ProposedPurchasePrice.HasValue)
            {
                proposal.ProductVariant.PurchasePrice = proposal.ProposedPurchasePrice;
            }
        }
        else
        {
            proposal.Product!.Price = proposal.ProposedSalePrice;
            if (proposal.ProposedPurchasePrice.HasValue)
            {
                proposal.Product.PurchasePrice = proposal.ProposedPurchasePrice;
            }
        }

        await RegisterPriceHistoryAsync(proposal.ProductId, proposal.ProductVariantId, "Sale", proposal.ProposedSalePrice, proposal.Id, cancellationToken);
        if (proposal.ProposedPurchasePrice.HasValue)
        {
            await RegisterPriceHistoryAsync(proposal.ProductId, proposal.ProductVariantId, "Purchase", proposal.ProposedPurchasePrice.Value, proposal.Id, cancellationToken);
        }

        proposal.Status = WorkflowStatus.QueuedForSync;
        proposal.SuperAdminApprovedByUserName = User.Identity?.Name ?? "unknown";
        proposal.SuperAdminApprovedAtUtc = DateTime.UtcNow;
        proposal.SuperAdminNote = request.Note?.Trim();
        proposal.QueuedForSyncAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _outboxService.QueueProductForCentralSyncAsync(proposal.ProductId, proposal.ProductVariantId, "PriceProposal", User.Identity?.Name ?? "unknown", cancellationToken);
        await AuditAsync("PriceProposal", proposal.Id, "SuperAdminApproveAndQueue", request.Note, cancellationToken);
        return Ok(await MapAsync(proposal.Id, cancellationToken));
    }

    [HttpPost("proposals/{id:long}/reject")]
    [Authorize(Policy = AuthorizationPolicyNames.PriceProposalManagerApproval)]
    public async Task<ActionResult<PriceProposalDto>> Reject(long id, [FromBody] RejectRequest request, CancellationToken cancellationToken)
    {
        var proposal = await _context.PriceProposals.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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
        await AuditAsync("PriceProposal", proposal.Id, "Reject", request.Reason, cancellationToken);
        return Ok(await MapAsync(proposal.Id, cancellationToken));
    }

    private bool CanSeePurchasePrice() => User.IsInRole(KharbarchiRoles.SuperAdmin) || User.IsInRole(KharbarchiRoles.LegacyAdmin) || User.IsInRole(KharbarchiRoles.PricingManager);

    private async Task<PriceProposalDto> MapAsync(long id, CancellationToken cancellationToken)
    {
        var canSeePurchasePrice = CanSeePurchasePrice();
        return await _context.PriceProposals
            .AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.ProductVariant)
            .Where(x => x.Id == id)
            .Select(x => new PriceProposalDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                ProductName = x.Product!.Name,
                VariantName = x.ProductVariant != null ? x.ProductVariant.Name : null,
                Sku = x.ProductVariant != null ? x.ProductVariant.Sku : x.Product.Sku,
                CurrentSalePrice = x.CurrentSalePrice,
                ProposedSalePrice = x.ProposedSalePrice,
                CurrentPurchasePrice = canSeePurchasePrice ? x.CurrentPurchasePrice : null,
                ProposedPurchasePrice = canSeePurchasePrice ? x.ProposedPurchasePrice : null,
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

    private async Task RegisterPriceHistoryAsync(
        int productId,
        int? productVariantId,
        string priceType,
        decimal amount,
        long proposalId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var currentRows = await _context.ProductPriceHistory
            .Where(x => x.ProductId == productId && x.ProductVariantId == productVariantId && x.PriceType == priceType && x.IsCurrent)
            .ToListAsync(cancellationToken);
        foreach (var row in currentRows)
        {
            row.IsCurrent = false;
            row.ValidToUtc = now;
        }

        _context.ProductPriceHistory.Add(new ProductPriceHistory
        {
            ProductId = productId,
            ProductVariantId = productVariantId,
            PriceType = priceType,
            Amount = amount,
            IsCurrent = true,
            ValidFromUtc = now,
            Source = $"PriceProposal:{proposalId}",
            ChangedByUserName = User.Identity?.Name ?? "unknown"
        });
    }
}
