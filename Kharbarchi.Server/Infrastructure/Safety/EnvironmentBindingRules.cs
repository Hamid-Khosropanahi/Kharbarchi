namespace Kharbarchi.Server.Infrastructure.Safety;

public static class EnvironmentBindingRules
{
    public const string DevelopmentEnvironmentName = "Development";
    public const string ProductionEnvironmentName = "Production";
    public const string LocalProfileEnvironmentType = "Local";
    public const string ProductionProfileEnvironmentType = "Production";
    public const string DevelopmentDatabaseName = "kharbarchi_local_fresh";
    public const string ProductionDatabaseName = "Kharbarchi_erp";
    public const string CanonicalProductionSiteUrl = "https://www.Kharbarchi.ir/";

    public static string NormalizeEnvironmentName(string? environmentName)
    {
        if (string.Equals(environmentName, DevelopmentEnvironmentName, StringComparison.OrdinalIgnoreCase))
        {
            return DevelopmentEnvironmentName;
        }

        if (string.Equals(environmentName, ProductionEnvironmentName, StringComparison.OrdinalIgnoreCase))
        {
            return ProductionEnvironmentName;
        }

        return string.IsNullOrWhiteSpace(environmentName) ? "[not-configured]" : environmentName.Trim();
    }

    public static string GetRequiredDatabaseName(string environmentName) =>
        environmentName switch
        {
            DevelopmentEnvironmentName => DevelopmentDatabaseName,
            ProductionEnvironmentName => ProductionDatabaseName,
            _ => throw new InvalidOperationException(
                $"EnvironmentSafetyGuard: ASP.NET Core environment '{environmentName}' is unsupported. "
                + "Set ASPNETCORE_ENVIRONMENT explicitly to Development or Production.")
        };

    public static string GetRequiredProfileEnvironmentType(string environmentName) =>
        environmentName switch
        {
            DevelopmentEnvironmentName => LocalProfileEnvironmentType,
            ProductionEnvironmentName => ProductionProfileEnvironmentType,
            _ => throw new InvalidOperationException(
                $"EnvironmentSafetyGuard: ASP.NET Core environment '{environmentName}' is unsupported. "
                + "Set ASPNETCORE_ENVIRONMENT explicitly to Development or Production.")
        };

    public static bool IsLocalDevelopmentUrl(string? value)
    {
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }

        var host = uri.Host;
        return uri.IsLoopback
            || host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".local", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsCanonicalProductionSiteUrl(string? value) =>
        string.Equals(value?.Trim(), CanonicalProductionSiteUrl, StringComparison.Ordinal);
}
