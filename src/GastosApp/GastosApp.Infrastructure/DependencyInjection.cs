using GastosApp.Application.Authentication;
using GastosApp.Application.Common;
using GastosApp.Infrastructure.Authentication;
using GastosApp.Infrastructure.Identity;
using GastosApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GastosApp.Infrastructure;

/// <summary>
/// Extension de IServiceCollection para registrar todos los servicios de Infrastructure.
/// La cadena de conexion se lee de la variable de entorno GASTOSAPP_DB
/// o de la clave ConnectionStrings:Default en la configuracion (sin credenciales en el repo).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("GASTOSAPP_DB")
            ?? configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "No se encontro la cadena de conexion. " +
                "Define la variable de entorno GASTOSAPP_DB o ConnectionStrings:Default.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false; // Habilitar en produccion
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        return services;
    }
}
