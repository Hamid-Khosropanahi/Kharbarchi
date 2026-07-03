using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kharbarchi.Server.Data;

public sealed class AppDbContextDesignFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__MySqlConnection")
            ?? "Server=127.0.0.1;Port=3306;Database=kharbarchi_design;Uid=design;Pwd=design;CharSet=utf8mb4;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySQL(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
