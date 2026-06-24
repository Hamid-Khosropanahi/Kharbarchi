namespace Kharbarchi.Server.Models;

public sealed class WooCommerceRuntimeSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public bool AllowInsecureLocalhostSsl { get; set; } = true;
}
