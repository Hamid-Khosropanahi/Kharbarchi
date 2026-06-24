namespace Kharbarchi.Shared.Contracts.WooCommerce;

public sealed class WooConnectionSettingsDto
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public bool HasConsumerSecret { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public bool AllowInsecureLocalhostSsl { get; set; } = true;
}

public sealed class WooConnectionTestRequest
{
    public string TestKind { get; set; } = "Products";
    public string? BaseUrl { get; set; }
    public string? ConsumerKey { get; set; }
    public string? ConsumerSecret { get; set; }
    public int? TimeoutSeconds { get; set; }
    public bool? AllowInsecureLocalhostSsl { get; set; }
}

public sealed class WooRawApiRequest
{
    public string Method { get; set; } = "GET";
    public string RelativeUrl { get; set; } = "/wp-json/wc/v3/products?per_page=1";
    public string? BodyJson { get; set; }
    public string? BaseUrl { get; set; }
    public string? ConsumerKey { get; set; }
    public string? ConsumerSecret { get; set; }
    public int? TimeoutSeconds { get; set; }
    public bool? AllowInsecureLocalhostSsl { get; set; }
}

public sealed class WooApiTestResultDto
{
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long ElapsedMilliseconds { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ResponseBody { get; set; } = string.Empty;
    public string CurlPreview { get; set; } = string.Empty;
}
