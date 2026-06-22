using System.Text.Json;
using Kharbarchi.Server.Data;
using Kharbarchi.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Services;

public sealed class SyncOutboxService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly AppDbContext _context;

    public SyncOutboxService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<long> QueueProductForCentralSyncAsync(int productId, int? variantId, string sourceWorkflow, string userName, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Commodity)
            .Include(x => x.Variants)
            .Include(x => x.ProductTags).ThenInclude(x => x.ProductTag)
            .Include(x => x.SpecValues).ThenInclude(x => x.SpecDefinition)
            .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken)
            ?? throw new InvalidOperationException("Product was not found.");

        ProductVariant? variant = null;
        if (variantId.HasValue)
        {
            variant = product.Variants.FirstOrDefault(x => x.Id == variantId.Value)
                ?? throw new InvalidOperationException("Product variant was not found.");
        }

        var payload = new
        {
            localProductId = product.Id,
            localVariantId = variant?.Id,
            wooCommerceProductId = variant?.WooCommerceProductId ?? product.WooCommerceProductId,
            wooCommerceVariationId = variant?.WooCommerceVariationId,
            productName = product.Name,
            variantName = variant?.Name,
            slug = product.Slug,
            sku = variant?.Sku ?? product.Sku,
            description = product.Description,
            category = product.Category?.Name,
            brand = product.Brand?.Name,
            commodity = product.Commodity?.Name,
            imageUrl = product.ImageUrl,
            price = variant?.Price ?? product.Price,
            oldPrice = variant?.OldPrice ?? product.OldPrice,
            stockQuantity = variant?.StockQuantity ?? product.StockQuantity,
            isAvailable = variant?.IsAvailable ?? product.IsAvailable,
            tags = product.ProductTags.Select(x => x.ProductTag!.Name).OrderBy(x => x).ToArray(),
            specs = product.SpecValues
                .OrderBy(x => x.SpecDefinition!.SortOrder)
                .Select(x => new { name = x.SpecDefinition!.Name, unit = x.SpecDefinition.Unit, value = x.Value })
                .ToArray()
        };

        var message = new SyncOutboxMessage
        {
            EventType = variant is null ? "WooCommerce.Product.Upsert" : "WooCommerce.ProductVariation.Upsert",
            AggregateType = variant is null ? "Product" : "ProductVariant",
            AggregateId = variant?.Id ?? product.Id,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            Status = OutboxStatus.Pending,
            QueuedByUserName = userName,
            SourceWorkflow = sourceWorkflow
        };

        _context.SyncOutboxMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
        return message.Id;
    }

    public async Task<IReadOnlyList<SyncOutboxMessage>> GetPendingForCentralAgentAsync(int take, string lockedBy, CancellationToken cancellationToken)
    {
        take = Math.Clamp(take, 1, 100);

        var messages = await _context.SyncOutboxMessages
            .Where(x => x.Status == OutboxStatus.Pending || x.Status == OutboxStatus.Failed)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.Status = OutboxStatus.Locked;
            message.LockedBy = lockedBy;
            message.LockedAtUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return messages;
    }

    public async Task MarkSentAsync(long id, string userName, string? note, CancellationToken cancellationToken)
    {
        var message = await _context.SyncOutboxMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Outbox message was not found.");

        message.Status = OutboxStatus.Sent;
        message.SentAtUtc = DateTime.UtcNow;
        message.LastError = note;
        message.LockedBy = userName;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(long id, string userName, string error, CancellationToken cancellationToken)
    {
        var message = await _context.SyncOutboxMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Outbox message was not found.");

        message.Status = OutboxStatus.Failed;
        message.RetryCount += 1;
        message.LastError = error;
        message.LockedBy = userName;
        message.LockedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
