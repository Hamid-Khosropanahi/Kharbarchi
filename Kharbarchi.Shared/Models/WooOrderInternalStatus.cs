namespace Kharbarchi.Shared.Models;

public static class WooOrderInternalStatus
{
    public const string Synced = "Synced";
    public const string AwaitingPayment = "AwaitingPayment";      // در انتظار پرداخت
    public const string PaymentVerificationRequired = "PaymentVerificationRequired";
    public const string PaymentConfirmed = "PaymentConfirmed";
    public const string ReadyToShip = "ReadyToShip";              // آماده ارسال
    public const string Shipped = "Shipped";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string PaymentFailed = "PaymentFailed";
}
