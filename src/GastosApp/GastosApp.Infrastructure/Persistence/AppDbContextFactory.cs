using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GastosApp.Infrastructure.Persistence;

/// <summary>
/// Fabrica de DbContext para las herramientas de diseno de EF Core (dotnet ef migrations).
/// Lee la cadena de conexion desde User Secrets, variables de entorno o el fallback de desarrollo.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Construye la configuracion igual que la Api: appsettings + User Secrets + env vars
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<AppDbContextFactory>(optional: true)
            .Build();

        var connectionString =
            Environment.GetEnvironmentVariable("GASTOSAPP_DB")
            ?? configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=gastosapp_dev;Username=postgres;Password=1234";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}
