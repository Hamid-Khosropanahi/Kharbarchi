namespace Kharbarchi.Server.Options;

public sealed class BarookOptions
{
    public const string SectionName = "Barook";

    public string CpgBaseUrl { get; set; } = "https://api.barook.tech/barook";
    public string CpgTerminalCode { get; set; } = string.Empty;
    public string CpgPassword { get; set; } = string.Empty;

    // مستند نسخه 7 مسیر /cpg/v1/redirect/{token} را نشان می‌دهد، اما پیام شما مسیر /cpg/redirect/TOKEN را داده است.
    // مسیر را از تنظیمات کنترل می‌کنیم تا بدون تغییر کد قابل اصلاح باشد.
    public string RedirectPathTemplate { get; set; } = "/cpg/redirect/{token}";
    public string StartPaymentPath { get; set; } = "/cpg/v2/start-payment";
    public string VerifyPaymentPath { get; set; } = "/cpg/v2/verify-payment";

    public int DefaultPaymentDayCount { get; set; } = 14;

    // مستند باروک مبلغ را با واحد ریال می‌خواهد. اگر قیمت‌های WooCommerce شما تومان است، مقدار را 10 بگذارید؛ اگر ریال است 1.
    public decimal AmountMultiplierToRial { get; set; } = 10;

    public int TimeoutSeconds { get; set; } = 45;
    public bool AllowInsecureLocalhostSsl { get; set; }
}
