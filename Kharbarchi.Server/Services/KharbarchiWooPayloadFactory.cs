using System.Globalization;
using System.Text.Json.Serialization;
using Kharbarchi.Shared.Models;

namespace Kharbarchi.Server.Services;

public sealed class KharbarchiWooPayloadFactory
{
    private readonly KharbarchiPriceControlService _priceControl;

    public KharbarchiWooPayloadFactory(KharbarchiPriceControlService priceControl)
    {
        _priceControl = priceControl;
    }

    public WooProductUpsertPayload BuildProductPayload(Product product, ProductWooControlProfile profile, long? wooCategoryId)
    {
        var snapshot = _priceControl.Apply(product, profile);
        var saleCredit = profile.SaleCreditPrice ?? product.Price;
        var saleCash = profile.SaleCashPrice;
        var status = product.IsAvailable && profile.PriceCheckStatus != "red" ? "publish" : "draft";
        var stock = Math.Max(0, product.StockQuantity);
        var meta = BuildMeta(product, profile, snapshot).ToList();

        var attributes = new List<WooProductAttributePayload>();
        if (product.Brand is not null)
        {
            attributes.Add(new WooProductAttributePayload("برند", true, false, new[] { product.Brand.Name }));
        }

        if (product.Commodity is not null)
        {
            attributes.Add(new WooProductAttributePayload("کالای پایه", true, false, new[] { product.Commodity.Name }));
        }

        if (!string.IsNullOrWhiteSpace(profile.PackageTitle))
        {
            attributes.Add(new WooProductAttributePayload("نوع بسته‌بندی", true, false, new[] { profile.PackageTitle }));
        }

        IReadOnlyList<WooIdReference> categories = wooCategoryId.HasValue && wooCategoryId.Value > 0
            ? new[] { new WooIdReference(wooCategoryId.Value) }
            : Array.Empty<WooIdReference>();

        return new WooProductUpsertPayload
        {
            Name = product.Name,
            Slug = product.Slug,
            Sku = product.Sku ?? string.Empty,
            Type = "simple",
            RegularPrice = ToWooMoney(saleCredit),
            SalePrice = string.Empty,
            Status = status,
            CatalogVisibility = status == "publish" ? "visible" : "hidden",
            Description = product.Description ?? string.Empty,
            ShortDescription = BuildShortDescription(product, profile, snapshot),
            ManageStock = true,
            StockQuantity = stock,
            StockStatus = stock > 0 && status == "publish" ? "instock" : "outofstock",
            Categories = categories,
            Attributes = attributes,
            MetaData = meta
        };
    }

    private static IEnumerable<WooMetaPayload> BuildMeta(Product product, ProductWooControlProfile profile, KharbarchiPriceControlSnapshot snapshot)
    {
        yield return Meta("_kharbarchi_sale_cash_price", profile.SaleCashPrice);
        yield return Meta("_kharbarchi_sale_credit_price", profile.SaleCreditPrice ?? product.Price);
        yield return Meta("_kharbarchi_buy_cash_price", profile.BuyCashPrice);
        yield return Meta("_kharbarchi_buy_credit_price", profile.BuyCreditPrice);
        yield return Meta("_kharbarchi_sale_cash_price_per_kg", profile.SaleCashPricePerKg);
        yield return Meta("_kharbarchi_sale_credit_price_per_kg", profile.SaleCreditPricePerKg);
        yield return Meta("_kharbarchi_buy_cash_price_per_kg", profile.BuyCashPricePerKg);
        yield return Meta("_kharbarchi_buy_credit_price_per_kg", profile.BuyCreditPricePerKg);

        yield return Meta("_khb_package_code", profile.PackageCode);
        yield return Meta("_khb_package_title", profile.PackageTitle);
        yield return Meta("_khb_package_group", profile.PackageGroup);
        yield return Meta("_khb_unit_weight", profile.UnitWeightKg);
        yield return Meta("_khb_product_carton_count", profile.ProductCartonCount);
        yield return Meta("_khb_bulk_weight_kg", profile.BulkWeightKg);
        yield return Meta("_khb_min_purchase_kg", profile.MinPurchaseKg);
        yield return Meta("_khb_image_tag", profile.ImageTag);
        yield return Meta("_kharbarchi_min_cartons", profile.MinCartons);
        yield return Meta("_kharbarchi_max_cartons", profile.MaxCartons);
        yield return Meta("_kharbarchi_carton_step", profile.CartonStep);
        yield return Meta("woodmart_price_unit_of_measure", profile.WoodmartPriceUnitOfMeasure);

        yield return Meta("_khb_price_source_mode", profile.PriceSourceMode);
        yield return Meta("_khb_expected_sale_credit_price", profile.ExpectedSaleCreditPrice);
        yield return Meta("_khb_expected_sale_cash_price", profile.ExpectedSaleCashPrice);
        yield return Meta("_khb_expected_buy_credit_price", profile.ExpectedBuyCreditPrice);
        yield return Meta("_khb_expected_buy_cash_price", profile.ExpectedBuyCashPrice);
        yield return Meta("_khb_sale_credit_diff", profile.SaleCreditDiff);
        yield return Meta("_khb_sale_cash_diff", profile.SaleCashDiff);
        yield return Meta("_khb_buy_credit_diff", profile.BuyCreditDiff);
        yield return Meta("_khb_buy_cash_diff", profile.BuyCashDiff);
        yield return Meta("_khb_price_check_status", profile.PriceCheckStatus);
        yield return Meta("_khb_price_check_code", profile.PriceCheckCode);
        yield return Meta("_khb_price_check_note", profile.PriceCheckNote);
        yield return Meta("_khb_price_check_percent", profile.PriceCheckPercent);
        yield return Meta("_khb_price_check_amount", profile.PriceCheckAmount);
        yield return Meta("_khb_total_weight_kg", snapshot.TotalWeightKg);
        yield return Meta("_khb_need_fix", profile.NeedFix ? "1" : "0");
        yield return Meta("_khb_fix_note", profile.PriceCheckNote);

        yield return Meta("_kharbarchi_brand_name", product.Brand?.Name);
        yield return Meta("_kharbarchi_brand_english_name", product.Brand?.Slug);
        yield return Meta("_kharbarchi_commodity_name", product.Commodity?.Name);
        yield return Meta("_kharbarchi_commodity_slug", product.Commodity?.Slug);
        yield return Meta("_kharbarchi_category_name", product.Category?.Name);
        yield return Meta("_kharbarchi_category_slug", product.Category?.Slug);
        yield return Meta("_khb_source_id", product.Id);
        yield return Meta("_khb_erp_product_id", product.Id);
        yield return Meta("_khb_erp_synced_at_utc", DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
    }

    private static string BuildShortDescription(Product product, ProductWooControlProfile profile, KharbarchiPriceControlSnapshot snapshot)
    {
        var lines = new List<string>
        {
            $"قیمت فروش کالا: {ToDisplayMoney(profile.SaleCreditPrice ?? product.Price)} تومان"
        };

        if (profile.SaleCashPrice.HasValue && (profile.SaleCreditPrice ?? product.Price) > profile.SaleCashPrice.Value)
        {
            var diff = (profile.SaleCreditPrice ?? product.Price) - profile.SaleCashPrice.Value;
            lines.Add($"تخفیف تسویه نقدی: {ToDisplayMoney(diff)} تومان");
        }

        if (profile.SaleCreditPricePerKg.HasValue)
        {
            lines.Add($"قیمت کیلویی فروش: {ToDisplayMoney(profile.SaleCreditPricePerKg.Value)} تومان");
        }

        if (profile.SaleCreditPricePerKg.HasValue && profile.SaleCashPricePerKg.HasValue && profile.SaleCreditPricePerKg.Value > profile.SaleCashPricePerKg.Value)
        {
            lines.Add($"تخفیف فروش نقد کیلویی: {ToDisplayMoney(profile.SaleCreditPricePerKg.Value - profile.SaleCashPricePerKg.Value)} تومان");
        }

        if (!string.IsNullOrWhiteSpace(profile.PackageTitle))
        {
            lines.Add($"تعداد در کارتن/کیلو خرید: {profile.PackageTitle}");
        }
        else if (snapshot.TotalWeightKg.HasValue)
        {
            lines.Add($"وزن خرید: {snapshot.TotalWeightKg.Value:0.###} کیلو");
        }

        return string.Join("<br />", lines);
    }

    private static WooMetaPayload Meta(string key, object? value)
    {
        return new WooMetaPayload(key, value switch
        {
            null => string.Empty,
            decimal d => ToWooMoney(d),
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            bool b => b ? "1" : "0",
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        });
    }

    private static string ToWooMoney(decimal? value)
    {
        return value.HasValue && value.Value > 0 ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) : string.Empty;
    }

    private static string ToDisplayMoney(decimal value)
    {
        return Math.Round(value, 0, MidpointRounding.AwayFromZero).ToString("N0", CultureInfo.InvariantCulture);
    }
}

public sealed record WooProductUpsertPayload
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; init; } = string.Empty;

    [JsonPropertyName("sku")]
    public string Sku { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = "simple";

    [JsonPropertyName("regular_price")]
    public string RegularPrice { get; init; } = string.Empty;

    [JsonPropertyName("sale_price")]
    public string? SalePrice { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "draft";

    [JsonPropertyName("catalog_visibility")]
    public string CatalogVisibility { get; init; } = "hidden";

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("short_description")]
    public string ShortDescription { get; init; } = string.Empty;

    [JsonPropertyName("manage_stock")]
    public bool ManageStock { get; init; } = true;

    [JsonPropertyName("stock_quantity")]
    public int StockQuantity { get; init; }

    [JsonPropertyName("stock_status")]
    public string StockStatus { get; init; } = "outofstock";

    [JsonPropertyName("categories")]
    public IReadOnlyList<WooIdReference> Categories { get; init; } = [];

    [JsonPropertyName("attributes")]
    public IReadOnlyList<WooProductAttributePayload> Attributes { get; init; } = [];

    [JsonPropertyName("meta_data")]
    public IReadOnlyList<WooMetaPayload> MetaData { get; init; } = [];
}

public sealed record WooIdReference([property: JsonPropertyName("id")] long Id);

public sealed record WooProductAttributePayload(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("visible")] bool Visible,
    [property: JsonPropertyName("variation")] bool Variation,
    [property: JsonPropertyName("options")] IReadOnlyList<string> Options);

public sealed record WooMetaPayload(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] string Value);
