using System.Globalization;
using Kharbarchi.Server.Contracts;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Server.Data;
using Kharbarchi.Server.Models;
using Kharbarchi.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Kharbarchi.Server.Services;

public sealed class AccountingReceiptService
{
    private readonly AppDbContext _context;
    private readonly WooCommerceApiClient _woo;

    public AccountingReceiptService(AppDbContext context, WooCommerceApiClient woo)
    {
        _context = context;
        _woo = woo;
    }

    public async Task<ManualPaymentReceiptDto?> CreateManualReceiptAsync(long orderId, CreateManualPaymentReceiptRequest request, string userName, CancellationToken cancellationToken)
    {
        var order = await _context.WooCommerceOrderSnapshots
            .Include(x => x.ManualReceipts)
            .FirstOrDefaultAsync(x => x.Id == orderId || x.WooCommerceOrderId == orderId, cancellationToken);

        if (order is null)
        {
            return null;
        }

        var existing = await _context.ManualPaymentReceipts.AsNoTracking()
            .AnyAsync(x => x.ReceiptNumber == request.ReceiptNumber.Trim(), cancellationToken);
        if (existing)
        {
            throw new InvalidOperationException("شماره رسید قبلاً ثبت شده است.");
        }

        var receipt = new ManualPaymentReceipt
        {
            WooCommerceOrderSnapshotId = order.Id,
            WooCommerceOrderId = order.WooCommerceOrderId,
            Amount = request.Amount,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            ReceiptNumber = request.ReceiptNumber.Trim(),
            PaymentSource = request.PaymentSource.Trim(),
            PaidAtUtc = request.PaidAtUtc?.ToUniversalTime() ?? DateTime.UtcNow,
            RegisteredByUserName = userName,
            Note = request.Note?.Trim(),
            SentToWooCommerce = false
        };

        _context.ManualPaymentReceipts.Add(receipt);
        order.PaymentStatus = PaymentStatusNames.ManualPaid;
        order.InternalStatus = WooOrderInternalStatus.ReadyToShip;
        order.ReadyToShipAtUtc = DateTime.UtcNow;
        order.LastActionByUserName = userName;
        order.LastActionNote = $"Manual receipt registered: {receipt.ReceiptNumber}";
        await _context.SaveChangesAsync(cancellationToken);

        if (request.SendToWooCommerce)
        {
            var txId = $"MANUAL-{receipt.ReceiptNumber}";
            await _woo.MarkOrderPaymentAsync(order.WooCommerceOrderId, new WooCommercePaymentUpdatePayload
            {
                SetPaid = true,
                Status = "processing",
                TransactionId = txId,
                MetaData =
                [
                    new WooCommerceMetaData("_khb_manual_receipt_number", receipt.ReceiptNumber),
                    new WooCommerceMetaData("_khb_manual_payment_source", receipt.PaymentSource),
                    new WooCommerceMetaData("_khb_manual_payment_amount", receipt.Amount.ToString("0.##", CultureInfo.InvariantCulture)),
                    new WooCommerceMetaData("_khb_internal_status", WooOrderInternalStatus.ReadyToShip)
                ]
            }, cancellationToken);

            await _woo.AddOrderNoteAsync(order.WooCommerceOrderId,
                new WooCommerceOrderNotePayload($"Manual payment receipt registered by accounting. Receipt={receipt.ReceiptNumber}; Amount={receipt.Amount:0.##} {receipt.Currency}."),
                cancellationToken);

            receipt.SentToWooCommerce = true;
            receipt.SentToWooCommerceAtUtc = DateTime.UtcNow;
            receipt.WooCommerceResponseSummary = "WooCommerce order marked as processing/set_paid by accounting.";
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new ManualPaymentReceiptDto(receipt.Id, receipt.Amount, receipt.Currency, receipt.ReceiptNumber, receipt.PaymentSource, receipt.PaidAtUtc, receipt.RegisteredByUserName, receipt.Note, receipt.SentToWooCommerce, receipt.CreatedAtUtc);
    }
}
