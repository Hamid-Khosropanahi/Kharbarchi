using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace Kharbarchi.Server.Infrastructure.Safety;

public sealed class EnvironmentSafetyGuard
{
    private readonly ILogger<EnvironmentSafetyGuard> _logger;
    private readonly IHostEnvironment _environment;

    public EnvironmentSafetyGuard(ILogger<EnvironmentSafetyGuard> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    private static string ToSafeEndpointForLog(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return "[not-configured]";

        if (!Uri.TryCreate(baseUrl.Trim(), UriKind.Absolute, out var uri))
        {
            return "[invalid-url]";
        }

        var scheme = uri.Scheme;
        var host = uri.Host;
        var port = uri.IsDefaultPort ? string.Empty : ":" + uri.Port.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return $"{scheme}://{host}{port}";
    }

    private static string DescribeWooSource(int profileId) =>
        profileId > 0
            ? $"WooCommerce profile Id={profileId} in table 'khb_woocommerce_connection_profiles'"
            : "global WooCommerce configuration (WooCommerce__EnvironmentType, WooCommerce__BaseUrl, WooCommerce__VerifySsl)";

    public void ValidateDatabaseConnectionString(string connectionString, string connectionStringName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string is empty.");
        }

        // Parse safely using the MySql connection string parser so we do not log secrets.
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var database = (builder.Database ?? string.Empty).Trim();
        var envName = EnvironmentBindingRules.NormalizeEnvironmentName(_environment.EnvironmentName);
        var requiredDatabase = EnvironmentBindingRules.GetRequiredDatabaseName(envName);

        // Log only environment name, connection string name and database name (no passwords or full connection string)
        _logger.LogInformation("EnvironmentSafetyGuard: Environment={Environment}, ConnectionStringName={Name}, Database={Database}", envName, connectionStringName, database);

        if (!string.Equals(database, requiredDatabase, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"EnvironmentSafetyGuard: ASPNETCORE_ENVIRONMENT={envName} requires database name exactly "
                + $"'{requiredDatabase}', but ConnectionStrings__{connectionStringName} selects '{database}'.");
        }
    }

    public void ValidateSiteUrl(string? siteUrl, string configurationName)
    {
        var envName = EnvironmentBindingRules.NormalizeEnvironmentName(_environment.EnvironmentName);
        _ = EnvironmentBindingRules.GetRequiredDatabaseName(envName);
        var url = siteUrl?.Trim() ?? string.Empty;

        if (envName == EnvironmentBindingRules.DevelopmentEnvironmentName)
        {
            if (!EnvironmentBindingRules.IsLocalDevelopmentUrl(url))
            {
                throw new InvalidOperationException(
                    $"EnvironmentSafetyGuard: ASPNETCORE_ENVIRONMENT=Development requires {configurationName} "
                    + "to be an absolute localhost, loopback, *.localhost, or *.local URL. "
                    + $"The Production URL '{EnvironmentBindingRules.CanonicalProductionSiteUrl}' is forbidden.");
            }

            return;
        }

        if (!EnvironmentBindingRules.IsCanonicalProductionSiteUrl(url))
        {
            throw new InvalidOperationException(
                $"EnvironmentSafetyGuard: ASPNETCORE_ENVIRONMENT=Production requires {configurationName} "
                + $"to be exactly '{EnvironmentBindingRules.CanonicalProductionSiteUrl}'.");
        }
    }

    public void ValidateWooProfile(
        string baseUrl,
        bool verifySsl,
        string environmentType,
        int profileId = 0,
        string? expectedProductionBaseUrl = null)
    {
        var envName = EnvironmentBindingRules.NormalizeEnvironmentName(_environment.EnvironmentName);
        var requiredProfileEnvironment = EnvironmentBindingRules.GetRequiredProfileEnvironmentType(envName);
        var url = (baseUrl ?? string.Empty).Trim();
        var profileEnvironment = (environmentType ?? string.Empty).Trim();
        var source = DescribeWooSource(profileId);

        // Log only environment and profile id + sanitized baseUrl (scheme://host:port)
        var safe = ToSafeEndpointForLog(url);
        _logger.LogDebug("EnvironmentSafetyGuard: Environment={Environment}, ProfileId={ProfileId}, BaseEndpoint={BaseEndpoint}", envName, profileId, safe);

        if (!string.Equals(profileEnvironment, requiredProfileEnvironment, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"EnvironmentSafetyGuard: ASPNETCORE_ENVIRONMENT={envName} requires {source} to have "
                + $"EnvironmentType='{requiredProfileEnvironment}', but its value is '{profileEnvironment}'.");
        }

        if (envName == EnvironmentBindingRules.DevelopmentEnvironmentName)
        {
            if (!EnvironmentBindingRules.IsLocalDevelopmentUrl(url))
            {
                throw new InvalidOperationException(
                    $"EnvironmentSafetyGuard: ASPNETCORE_ENVIRONMENT=Development requires {source} BaseUrl "
                    + "to be an absolute localhost, loopback, *.localhost, or *.local URL. "
                    + $"The Production URL '{EnvironmentBindingRules.CanonicalProductionSiteUrl}' is forbidden.");
            }

            return;
        }

        if (!EnvironmentBindingRules.IsCanonicalProductionSiteUrl(url))
        {
            throw new InvalidOperationException(
                $"EnvironmentSafetyGuard: ASPNETCORE_ENVIRONMENT=Production requires {source} BaseUrl "
                + $"to be exactly '{EnvironmentBindingRules.CanonicalProductionSiteUrl}'.");
        }

        if (!verifySsl)
        {
            throw new InvalidOperationException(
                $"EnvironmentSafetyGuard: {source} must have VerifySsl=true in Production.");
        }

        if (!string.IsNullOrWhiteSpace(expectedProductionBaseUrl)
            && !EnvironmentBindingRules.IsCanonicalProductionSiteUrl(expectedProductionBaseUrl))
        {
            throw new InvalidOperationException(
                "EnvironmentSafetyGuard: Production Site__PublicUrl and WooCommerce__BaseUrl must both be exactly "
                + $"'{EnvironmentBindingRules.CanonicalProductionSiteUrl}'.");
        }
    }
}
