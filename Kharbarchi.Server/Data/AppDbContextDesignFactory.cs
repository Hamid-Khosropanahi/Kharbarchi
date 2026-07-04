using Kharbarchi.Server.Infrastructure.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kharbarchi.Server.Data;

public sealed class AppDbContextDesignFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MySqlConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Set ConnectionStrings__MySqlConnection before running dotnet ef commands. Do not hard-code database credentials in the repository.");
        }

        // Validate database name for design-time to avoid accidental connections to production ERP DB
        var env = EnvironmentBindingRules.NormalizeEnvironmentName(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? EnvironmentBindingRules.DevelopmentEnvironmentName);

        try
        {
            var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(connectionString);
            var db = (builder.Database ?? string.Empty).Trim();
            var requiredDatabase = EnvironmentBindingRules.GetRequiredDatabaseName(env);
            if (!string.Equals(db, requiredDatabase, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Design-time operations with ASPNETCORE_ENVIRONMENT={env} require database name "
                    + $"exactly '{requiredDatabase}', but ConnectionStrings__MySqlConnection selects '{db}'.");
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch
        {
            // If parsing fails, avoid revealing connection string content; surface a generic error.
            throw new InvalidOperationException("ConnectionStrings__MySqlConnection is invalid or could not be parsed.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySQL(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
