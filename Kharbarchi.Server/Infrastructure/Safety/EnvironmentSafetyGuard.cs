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

    public void ValidateDatabaseConnectionString(string connectionString, string connectionStringName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string is empty.");
        }

        // Parse safely using the MySql connection string parser so we do not log secrets.
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var database = (builder.Database ?? string.Empty).Trim();
        var envName = string.IsNullOrWhiteSpace(_environment.EnvironmentName) ? "Development" : _environment.EnvironmentName;

        // Log only environment name, connection string name and database name (no passwords or full connection string)
        _logger.LogInformation("EnvironmentSafetyGuard: Environment={Environment}, ConnectionStringName={Name}, Database={Database}", envName, connectionStringName, database);

        if (string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(database, "kharbarchi_erp", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("EnvironmentSafetyGuard: In Development environment connecting to 'kharbarchi_erp' is not allowed.");
            }
            // kharbarchi_local and kharbarchi_dev are allowed in Development
        }

        if (string.Equals(envName, "Production", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(database, "kharbarchi_local", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(database, "kharbarchi_dev", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("EnvironmentSafetyGuard: In Production environment connecting to local/dev databases is not allowed.");
            }
        }
    }

    public void ValidateWooProfile(string baseUrl, bool verifySsl, string environmentType, int profileId = 0)
    {
        var envName = string.IsNullOrWhiteSpace(_environment.EnvironmentName) ? "Development" : _environment.EnvironmentName;
        // Normalize
        var url = (baseUrl ?? string.Empty).Trim();

        // Log only environment and profile id + sanitized baseUrl (scheme://host:port)
        var safe = ToSafeEndpointForLog(url);
        _logger.LogDebug("EnvironmentSafetyGuard: Environment={Environment}, ProfileId={ProfileId}, BaseEndpoint={BaseEndpoint}", envName, profileId, safe);

        if (string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            // Development host must not be pointed at a Production-profile
            if (string.Equals(environmentType, "Production", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("EnvironmentSafetyGuard: Running in Development while WooCommerce profile EnvironmentType is 'Production' is not allowed.");
            }

            if (url.Contains("kharbarchi.ir", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("EnvironmentSafetyGuard: In Development environment syncing/testing to production WooCommerce (kharbarchi.ir) is blocked.");
            }

            // localhost allowed for Local profiles
            return;
        }

        if (string.Equals(envName, "Production", StringComparison.OrdinalIgnoreCase))
        {
            // Production host must not use Local profiles
            if (string.Equals(environmentType, "Local", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("EnvironmentSafetyGuard: Running in Production while WooCommerce profile EnvironmentType is 'Local' is not allowed.");
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException("EnvironmentSafetyGuard: WooCommerce BaseUrl is not a valid absolute URL.");
            }

            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("EnvironmentSafetyGuard: Production WooCommerce profile must use HTTPS.");
            }

            if (!verifySsl)
            {
                throw new InvalidOperationException("EnvironmentSafetyGuard: Production WooCommerce profile must have VerifySsl=true.");
            }

            if (uri.IsLoopback || uri.Host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("EnvironmentSafetyGuard: Production environment must not use local WooCommerce profiles.");
            }

            return;
        }
    }
}
