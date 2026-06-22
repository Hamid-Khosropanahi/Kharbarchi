using System.Globalization;
using System.Text.Json;
using Kharbarchi.Server.Contracts;
using Kharbarchi.Shared.Contracts;
using Kharbarchi.Server.Data;
using Kharbarchi.Server.Models;
using Kharbarchi.Server.Options;
using Kharbarchi.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kharbarchi.Server.Services;

public sealed class BarookPaymentService
{
    private readonly AppDbContext _context;
    private readonly BarookCpgClient _barook;
    private readonly WooCommerceApiClient _woo;
    private readonly BarookOptions _options;

    public BarookPaymentService(AppDbContext context, BarookCpgClient barook, WooCommerceApiClient woo, IOptions<BarookOptions> options)
    {
        _context = context;
        _barook = barook;
        _woo = woo;
        _options = options.Value;
    }

    public async Task<StartBarookPaymentResponse?> StartPaymentAsync(long orderId, StartBarookPaymentRequest request, string userName, CancellationToken cancellationToken)
    {
        var order = await _context.WooCommerceOrderSnapshots
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == orderId || x.WooCommerceOrderId == orderId, cancellationToken);

        if (order is null)
        {
            return null;
        }

        if (order.Items.Count == 0)
        {
            throw new InvalidOperationException("این سفارش اقلام ندارد و قابل ارسال به باروک نیست.");
        }

        var externalCode = $"KHB-WOO-{order.WooCommerceOrderId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var barookTotalAmount = Math.Round(order.TotalAmount * _options.AmountMultiplierToRial, 0);
        var items = order.Items.Select(x => new BarookSaleItemClientDto(
            x.Name,
            x.Quantity,
            string.IsNullOrWhiteSpace(x.UnitType) ? "کارتن" : x.UnitType,
            Math.Round((x.UnitPrice <= 0 ? x.LineTotal : x.UnitPrice) * _options.AmountMultiplierToRial, 0))).ToList();

        var clientRequest = new BarookStartPaymentClientRequest(
            TerminalCode: string.Empty,
            Password: string.Empty,
            TotalAmount: barookTotalAmount,
            ExternalCode: externalCode,
            PaymentMonthCount: request.PaymentMonthCount,
            PaymentDayCount: request.PaymentDayCount ?? (request.PaymentMonthCount.HasValue ? null : _options.DefaultPaymentDayCount),
            OwnerName: request.OwnerName.Trim(),
            OwnerMobile: request.OwnerMobile?.Trim(),
            OwnerNationalCode: request.OwnerNationalCode.Trim(),
            RedirectUrl: request.RedirectUrl.Trim(),
            SaleItemDto: items,
            BranchCode: request.BranchCode?.Trim(),
            BusinessServiceSlug: request.BusinessServiceSlug?.Trim(),
            Attributes: new Dictionary<string, string>
            {
                ["wooCommerceOrderId"] = order.WooCommerceOrderId.ToString(CultureInfo.InvariantCulture),
                ["wooCommerceOrderNumber"] = order.WooCommerceOrderNumber,
                ["kharbarchiLocalOrderId"] = order.Id.ToString(CultureInfo.InvariantCulture)
            });

        var session = new BarookPaymentSession
        {
            WooCommerceOrderSnapshotId = order.Id,
            WooCommerceOrderId = order.WooCommerceOrderId,
            ExternalCode = externalCode,
            Amount = barookTotalAmount,
            Currency = order.Currency ?? "IRR",
            CreatedByUserName = userName,
            StartRequestJson = JsonSerializer.Serialize(clientRequest, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            BarookStatus = "CREATED"
        };

        _context.BarookPaymentSessions.Add(session);
        order.InternalStatus = WooOrderInternalStatus.AwaitingPayment;
        order.PaymentStatus = PaymentStatusNames.AwaitingPayment;
        order.LastActionByUserName = userName;
        order.LastActionNote = request.Note?.Trim();
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _barook.StartPaymentAsync(clientRequest, cancellationToken);
        session.Token = result.Token;
        session.TokenExpireDateUtc = result.ExpireDateUtc?.ToUniversalTime();
        session.StartResponseJson = result.RawJson;
        session.PaymentUrl = string.IsNullOrWhiteSpace(result.Token) ? null : _barook.BuildRedirectUrl(result.Token);
        await _context.SaveChangesAsync(cancellationToken);

        await _woo.AddOrderNoteAsync(order.WooCommerceOrderId,
            new WooCommerceOrderNotePayload($"Barook payment started by {userName}. ExternalCode={externalCode}. PaymentUrl={session.PaymentUrl}", false),
            cancellationToken);

        return new StartBarookPaymentResponse(session.Id, externalCode, session.Token, session.TokenExpireDateUtc, session.PaymentUrl, "لینک پرداخت باروک ساخته شد.");
    }

    public async Task<bool> MarkLinkSentAsync(long sessionId, MarkBarookPaymentLinkSentRequest request, string userName, CancellationToken cancellationToken)
    {
        var session = await _context.BarookPaymentSessions
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

        if (session is null)
        {
            return false;
        }

        session.LinkSentAtUtc = DateTime.UtcNow;
        if (session.Order is not null)
        {
            session.Order.LastActionByUserName = userName;
            session.Order.LastActionNote = string.IsNullOrWhiteSpace(request.Note) ? "Payment link sent." : request.Note.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<VerifyBarookPaymentResponse?> VerifyPaymentAsync(long sessionId, VerifyBarookPaymentRequest request, string userName, CancellationToken cancellationToken)
    {
        var session = await _context.BarookPaymentSessions
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

        if (session is null || session.Order is null)
        {
            return null;
        }

        var token = string.IsNullOrWhiteSpace(request.Token) ? session.Token : request.Token.Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("توکن پرداخت برای استعلام موجود نیست.");
        }

        var result = await _barook.VerifyPaymentAsync(session.ExternalCode, token, cancellationToken);
        var status = (result.Status ?? string.Empty).Trim().ToUpperInvariant();
        var isPaid = status == "COMPLETED";

        session.VerifiedAtUtc = DateTime.UtcNow;
        session.VerifiedByUserName = userName;
        session.BarookStatus = string.IsNullOrWhiteSpace(status) ? result.Status : status;
        session.ReferenceNumber = result.ReferenceNumber;
        session.MaskedCardNumber = MaskCard(result.CardNumber);
        session.TransactionId = result.TransactionId ?? result.ReferenceNumber ?? session.ExternalCode;
        session.VerifyResponseJson = result.RawJson;
        session.IsCompleted = isPaid;
        session.PaidAtUtc = isPaid ? DateTime.UtcNow : session.PaidAtUtc;

        session.Order.LastPaymentCheckedAtUtc = DateTime.UtcNow;
        session.Order.LastActionByUserName = userName;

        if (isPaid)
        {
            if (result.TotalAmount.HasValue && Math.Abs(result.TotalAmount.Value - session.Amount) > 1)
            {
                session.LastError = $"مبلغ دریافتی باروک با مبلغ سفارش برابر نیست. Barook={result.TotalAmount}, Order={session.Amount}";
                session.Order.InternalStatus = WooOrderInternalStatus.PaymentVerificationRequired;
                await _context.SaveChangesAsync(cancellationToken);
                return new VerifyBarookPaymentResponse(session.Id, session.ExternalCode, session.BarookStatus, false, session.TransactionId, session.LastError);
            }

            session.Order.PaymentStatus = PaymentStatusNames.BarookPaid;
            session.Order.InternalStatus = WooOrderInternalStatus.ReadyToShip;
            session.Order.ReadyToShipAtUtc = DateTime.UtcNow;
            session.Order.TransactionId = session.TransactionId;

            _context.GatewayPaymentReceipts.Add(new GatewayPaymentReceipt
            {
                WooCommerceOrderId = session.Order.WooCommerceOrderId,
                LocalOrderId = null,
                Amount = session.Amount,
                Currency = session.Currency,
                GatewayName = "Barook",
                TransactionId = session.TransactionId ?? session.ExternalCode,
                IdempotencyKey = $"BAROOK-{session.ExternalCode}",
                PaymentStatus = "Paid",
                GatewayRawStatus = session.BarookStatus,
                PaidAtUtc = session.PaidAtUtc,
                Note = $"Barook verified. ExternalCode={session.ExternalCode}; Reference={session.ReferenceNumber}; Card={session.MaskedCardNumber}",
                RequestedByUserName = userName,
                SentToWooCommerce = false
            });

            await _context.SaveChangesAsync(cancellationToken);

            if (request.SendResultToWooCommerce)
            {
                var payload = new WooCommercePaymentUpdatePayload
                {
                    SetPaid = true,
                    Status = "processing",
                    TransactionId = session.TransactionId ?? session.ExternalCode,
                    MetaData =
                    [
                        new WooCommerceMetaData("_khb_gateway_name", "Barook"),
                        new WooCommerceMetaData("_khb_barook_external_code", session.ExternalCode),
                        new WooCommerceMetaData("_khb_barook_reference", session.ReferenceNumber ?? string.Empty),
                        new WooCommerceMetaData("_khb_barook_status", session.BarookStatus ?? string.Empty),
                        new WooCommerceMetaData("_khb_internal_status", WooOrderInternalStatus.ReadyToShip),
                        new WooCommerceMetaData("_khb_paid_at_utc", (session.PaidAtUtc ?? DateTime.UtcNow).ToString("O", CultureInfo.InvariantCulture))
                    ]
                };

                await _woo.MarkOrderPaymentAsync(session.Order.WooCommerceOrderId, payload, cancellationToken);
                await _woo.AddOrderNoteAsync(session.Order.WooCommerceOrderId,
                    new WooCommerceOrderNotePayload($"Barook payment verified. ExternalCode={session.ExternalCode}; Reference={session.ReferenceNumber}; Status={session.BarookStatus}; Order is ready to ship."),
                    cancellationToken);

                var receipt = await _context.GatewayPaymentReceipts.FirstAsync(x => x.IdempotencyKey == $"BAROOK-{session.ExternalCode}", cancellationToken);
                receipt.SentToWooCommerce = true;
                receipt.SentToWooCommerceAtUtc = DateTime.UtcNow;
                receipt.WooCommerceResponseSummary = "WooCommerce order marked as processing/set_paid.";
            }
        }
        else
        {
            session.Order.InternalStatus = WooOrderInternalStatus.AwaitingPayment;
            session.Order.PaymentStatus = PaymentStatusNames.AwaitingPayment;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return new VerifyBarookPaymentResponse(session.Id, session.ExternalCode, session.BarookStatus, isPaid, session.TransactionId, isPaid ? "پرداخت تایید شد و سفارش آماده ارسال شد." : "پرداخت هنوز کامل نشده است.");
    }

    private static string? MaskCard(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length < 10) return value;
        return digits[..6] + "******" + digits[^4..];
    }
}
