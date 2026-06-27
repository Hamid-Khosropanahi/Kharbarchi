using System.Globalization;
using Kharbarchi.Shared.Models;

namespace Kharbarchi.Server.Services;

public sealed class KharbarchiPriceControlService
{
    private const decimal GreenPercentTolerance = 0.005m;
    private const decimal YellowPercentTolerance = 0.02m;
    private const decimal GreenAmountTolerance = 5000m;

    public KharbarchiPriceControlSnapshot Apply(Product product, ProductWooControlProfile profile)
    {
        var notes = new List<string>();
        var codes = new List<string>();
        var packageGroup = NormalizePackageGroup(profile.PackageGroup);
        profile.PackageGroup = packageGroup;
        profile.MinCartons = Math.Max(1, profile.MinCartons);
        profile.CartonStep = Math.Max(1, profile.CartonStep);
        profile.WoodmartPriceUnitOfMeasure = NormalizeUnitMeasure(profile.WoodmartPriceUnitOfMeasure, packageGroup);
        profile.SaleUnit = NormalizeSaleUnit(profile.SaleUnit, packageGroup);

        var totalWeight = CalculateTotalWeight(profile, packageGroup, codes, notes);
        var saleCredit = profile.SaleCreditPrice ?? (product.Price > 0 ? product.Price : null);
        var saleCash = profile.SaleCashPrice;
        var buyCredit = profile.BuyCreditPrice ?? profile.BuyCashPrice;
        var buyCash = profile.BuyCashPrice;

        profile.ExpectedSaleCreditPrice = CalculateExpected(profile.SaleCreditPricePerKg, totalWeight);
        profile.ExpectedSaleCashPrice = CalculateExpected(profile.SaleCashPricePerKg, totalWeight);
        profile.ExpectedBuyCreditPrice = CalculateExpected(profile.BuyCreditPricePerKg, totalWeight);
        profile.ExpectedBuyCashPrice = CalculateExpected(profile.BuyCashPricePerKg, totalWeight);

        profile.SaleCreditDiff = Difference(saleCredit, profile.ExpectedSaleCreditPrice);
        profile.SaleCashDiff = Difference(saleCash, profile.ExpectedSaleCashPrice);
        profile.BuyCreditDiff = Difference(buyCredit, profile.ExpectedBuyCreditPrice);
        profile.BuyCashDiff = Difference(buyCash, profile.ExpectedBuyCashPrice);

        if (saleCredit is null or <= 0)
        {
            AddIssue(codes, notes, "MISSING_SALE_CREDIT_PRICE", "قیمت فروش شرایطی خالی یا صفر است.");
        }

        if (saleCash is null or <= 0)
        {
            AddIssue(codes, notes, "MISSING_SALE_CASH_PRICE", "قیمت فروش نقدی خالی یا صفر است.");
        }

        if (buyCredit is null or <= 0)
        {
            AddIssue(codes, notes, "MISSING_BUY_CREDIT_PRICE", "قیمت خرید شرایطی خالی یا صفر است.");
        }

        if (buyCash is null or <= 0)
        {
            AddIssue(codes, notes, "MISSING_BUY_CASH_PRICE", "قیمت خرید نقدی خالی یا صفر است.");
        }

        if (saleCash.HasValue && saleCredit.HasValue && saleCash.Value > saleCredit.Value)
        {
            AddIssue(codes, notes, "CASH_GREATER_THAN_CREDIT", "قیمت نقدی نباید از قیمت شرایطی بیشتر باشد.");
        }

        var saleCreditSeverity = EvaluateDiff(saleCredit, profile.ExpectedSaleCreditPrice, "SALE_CREDIT", codes, notes);
        var saleCashSeverity = EvaluateDiff(saleCash, profile.ExpectedSaleCashPrice, "SALE_CASH", codes, notes);
        var buyCreditSeverity = EvaluateDiff(buyCredit, profile.ExpectedBuyCreditPrice, "BUY_CREDIT", codes, notes);
        var buyCashSeverity = EvaluateDiff(buyCash, profile.ExpectedBuyCashPrice, "BUY_CASH", codes, notes);

        var priceCheck = CalculateWorstDiff(profile, saleCredit, saleCash, buyCredit, buyCash);
        profile.PriceCheckAmount = priceCheck.Amount;
        profile.PriceCheckPercent = priceCheck.Percent;

        var hasRedCode = codes.Any(IsRedCode);
        var maxSeverity = new[] { saleCreditSeverity, saleCashSeverity, buyCreditSeverity, buyCashSeverity }.Max();

        if (hasRedCode || maxSeverity == PriceControlSeverity.Red)
        {
            profile.PriceCheckStatus = "red";
            if (!codes.Contains("DRAFT_REQUIRED", StringComparer.OrdinalIgnoreCase)) codes.Add("DRAFT_REQUIRED");
            profile.AutoDraftRequired = true;
        }
        else if (maxSeverity == PriceControlSeverity.Yellow || codes.Count > 0)
        {
            profile.PriceCheckStatus = "yellow";
            profile.AutoDraftRequired = false;
        }
        else
        {
            profile.PriceCheckStatus = "green";
            if (!codes.Contains("OK", StringComparer.OrdinalIgnoreCase)) codes.Add("OK");
            notes.Add("داده‌های قیمت، وزن و بسته‌بندی با تلرانس تعریف‌شده سازگار هستند.");
            profile.AutoDraftRequired = false;
        }

        profile.PriceCheckCode = string.Join('|', codes.Distinct(StringComparer.OrdinalIgnoreCase));
        profile.PriceCheckNote = string.Join(" ", notes.Distinct());
        profile.NeedFix = profile.PriceCheckStatus != "green";
        profile.UpdatedAtUtc = DateTime.UtcNow;

        product.Price = RoundMoney(saleCredit ?? 0);
        product.OldPrice = null;
        product.PurchasePrice = buyCredit ?? buyCash;
        product.IsAvailable = product.IsAvailable && !profile.AutoDraftRequired;
        product.UpdatedAtUtc = DateTime.UtcNow;

        return new KharbarchiPriceControlSnapshot(
            profile.PriceCheckStatus,
            profile.PriceCheckCode,
            profile.PriceCheckNote,
            totalWeight,
            profile.PriceCheckAmount,
            profile.PriceCheckPercent);
    }

    public decimal? CalculateTotalWeight(ProductWooControlProfile profile)
    {
        var codes = new List<string>();
        var notes = new List<string>();
        return CalculateTotalWeight(profile, NormalizePackageGroup(profile.PackageGroup), codes, notes);
    }

    private static decimal? CalculateTotalWeight(ProductWooControlProfile profile, string packageGroup, List<string> codes, List<string> notes)
    {
        if (packageGroup == "retail")
        {
            if (profile.UnitWeightKg is null or <= 0)
            {
                AddIssue(codes, notes, "MISSING_UNIT_WEIGHT", "وزن هر بسته برای محصول بسته‌بندی خالی است.");
            }

            if (profile.ProductCartonCount is null or <= 0)
            {
                AddIssue(codes, notes, "MISSING_CARTON_COUNT", "تعداد بسته در کارتن خالی است.");
            }

            if (profile.UnitWeightKg is > 0 && profile.ProductCartonCount is > 0)
            {
                return Math.Round(profile.UnitWeightKg.Value * profile.ProductCartonCount.Value, 6);
            }

            return null;
        }

        if (packageGroup == "bulk")
        {
            if (profile.BulkWeightKg is null or <= 0)
            {
                AddIssue(codes, notes, "MISSING_BULK_WEIGHT", "وزن کیسه/گونی فله خالی است.");
                return null;
            }

            if (profile.MinPurchaseKg is null or <= 0)
            {
                AddIssue(codes, notes, "MISSING_MIN_PURCHASE", "حداقل خرید کیلویی برای فله مشخص نشده است.");
            }

            return profile.BulkWeightKg.Value;
        }

        if (profile.UnitWeightKg is > 0)
        {
            return profile.ProductCartonCount is > 0
                ? Math.Round(profile.UnitWeightKg.Value * profile.ProductCartonCount.Value, 6)
                : profile.UnitWeightKg;
        }

        AddIssue(codes, notes, "MISSING_WEIGHT", "وزن محصول مشخص نیست.");
        return null;
    }

    private static decimal? CalculateExpected(decimal? perKg, decimal? totalWeight)
    {
        if (perKg is null or <= 0 || totalWeight is null or <= 0) return null;
        return RoundMoney(perKg.Value * totalWeight.Value);
    }

    private static decimal? Difference(decimal? actual, decimal? expected)
    {
        if (actual is null || expected is null) return null;
        return RoundMoney(actual.Value - expected.Value);
    }

    private static PriceControlSeverity EvaluateDiff(decimal? actual, decimal? expected, string name, List<string> codes, List<string> notes)
    {
        if (actual is null or <= 0 || expected is null or <= 0)
        {
            return PriceControlSeverity.Green;
        }

        var amount = Math.Abs(actual.Value - expected.Value);
        var percent = expected.Value == 0 ? 0 : amount / expected.Value;
        var greenLimit = Math.Max(GreenAmountTolerance, expected.Value * GreenPercentTolerance);

        if (amount <= greenLimit)
        {
            if (amount > 0)
            {
                AddIssue(codes, notes, "ROUNDING_DIFF", $"اختلاف {name} در محدوده گرد کردن است: {FormatMoney(amount)}.");
            }
            return PriceControlSeverity.Green;
        }

        if (percent <= YellowPercentTolerance)
        {
            AddIssue(codes, notes, "MINOR_PRICE_DIFF", $"اختلاف {name} نیازمند بررسی است: {FormatMoney(amount)}.");
            return PriceControlSeverity.Yellow;
        }

        AddIssue(codes, notes, "MAJOR_PRICE_DIFF", $"اختلاف جدی {name}: {FormatMoney(amount)}.");
        AddIssue(codes, notes, "FINAL_PRICE_MISMATCH", "قیمت نهایی با قیمت کیلویی و وزن سازگار نیست.");
        return PriceControlSeverity.Red;
    }

    private static (decimal? Amount, decimal? Percent) CalculateWorstDiff(ProductWooControlProfile profile, decimal? saleCredit, decimal? saleCash, decimal? buyCredit, decimal? buyCash)
    {
        var pairs = new[]
        {
            (actual: saleCredit, expected: profile.ExpectedSaleCreditPrice),
            (actual: saleCash, expected: profile.ExpectedSaleCashPrice),
            (actual: buyCredit, expected: profile.ExpectedBuyCreditPrice),
            (actual: buyCash, expected: profile.ExpectedBuyCashPrice)
        };

        decimal? worstAmount = null;
        decimal? worstPercent = null;

        foreach (var pair in pairs)
        {
            if (pair.actual is null or <= 0 || pair.expected is null or <= 0) continue;
            var amount = Math.Abs(pair.actual.Value - pair.expected.Value);
            var percent = pair.expected.Value == 0 ? 0 : amount / pair.expected.Value * 100m;
            if (worstAmount is null || amount > worstAmount.Value)
            {
                worstAmount = RoundMoney(amount);
                worstPercent = Math.Round(percent, 4);
            }
        }

        return (worstAmount, worstPercent);
    }

    private static void AddIssue(List<string> codes, List<string> notes, string code, string note)
    {
        if (!codes.Contains(code, StringComparer.OrdinalIgnoreCase)) codes.Add(code);
        if (!notes.Contains(note, StringComparer.OrdinalIgnoreCase)) notes.Add(note);
    }

    private static bool IsRedCode(string code)
    {
        return code is "MISSING_SALE_CREDIT_PRICE" or "MISSING_WEIGHT" or "MISSING_UNIT_WEIGHT" or "MISSING_BULK_WEIGHT" or "MISSING_CARTON_COUNT" or "CASH_GREATER_THAN_CREDIT" or "MAJOR_PRICE_DIFF" or "FINAL_PRICE_MISMATCH" or "INVALID_PACKAGE_CODE";
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 0, MidpointRounding.AwayFromZero);

    private static string NormalizePackageGroup(string? value)
    {
        var v = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (v is "retail" or "package" or "pack" or "carton") return "retail";
        if (v is "bulk" or "fله" or "fleh" or "bag") return "bulk";
        return "none";
    }

    private static string NormalizeSaleUnit(string? value, string group)
    {
        if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
        return group == "bulk" ? "bag" : group == "retail" ? "carton" : "item";
    }

    private static string NormalizeUnitMeasure(string? value, string group)
    {
        if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
        return group == "bulk" ? "گونی" : group == "retail" ? "کارتن" : "عدد";
    }

    private static string FormatMoney(decimal value) => value.ToString("N0", CultureInfo.InvariantCulture);

    private enum PriceControlSeverity
    {
        Green = 0,
        Yellow = 1,
        Red = 2
    }
}

public sealed record KharbarchiPriceControlSnapshot(
    string Status,
    string Code,
    string? Note,
    decimal? TotalWeightKg,
    decimal? DiffAmount,
    decimal? DiffPercent);
