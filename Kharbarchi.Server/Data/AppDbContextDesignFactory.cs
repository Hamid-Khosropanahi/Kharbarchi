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
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                  ?? "Development";

        try
        {
            var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(connectionString);
            var db = (builder.Database ?? string.Empty).Trim();

            if (string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(db, "kharbarchi_erp", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Design-time operations in Development must not target 'kharbarchi_erp'. Set a local or dev database.");
            }

            if (string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(db, "kharbarchi_local", StringComparison.OrdinalIgnoreCase) || string.Equals(db, "kharbarchi_dev", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Design-time operations in Production must not target local/dev databases.");
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
