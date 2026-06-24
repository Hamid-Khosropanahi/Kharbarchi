namespace Kharbarchi.Shared.Contracts.WooCommerce;

public sealed class WooDefaultImportRequestDto
{
    public bool ImportProducts { get; set; } = true;
    public bool ImportCategories { get; set; } = true;
    public bool ImportOrders { get; set; } = false;
    public int PerPage { get; set; } = 100;
    public int MaxPages { get; set; } = 10;
    public string? BaseUrl { get; set; }
    public string? ConsumerKey { get; set; }
    public string? ConsumerSecret { get; set; }
    public int? TimeoutSeconds { get; set; }
    public bool? AllowInsecureLocalhostSsl { get; set; }
}

public sealed class WooDefaultImportResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProductsImported { get; set; }
    public int CategoriesImported { get; set; }
    public int OrdersImported { get; set; }
    public int ApiCalls { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public string TableName { get; set; } = "khb_imported_woocommerce_records";
    public string RawLog { get; set; } = string.Empty;
}
