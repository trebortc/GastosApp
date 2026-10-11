using Microsoft.EntityFrameworkCore;

namespace GastosApp.Infrastructure.Persistence;

/// <summary>
/// Contexto principal de Entity Framework Core conectado a PostgreSQL.
/// La cadena de conexión se inyecta desde variables de entorno o secretos del proveedor de alojamiento.
/// Nunca almacenes credenciales reales en appsettings.json ni en el repositorio.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automáticamente todas las configuraciones IEntityTypeConfiguration<T>
        // que se encuentren en este ensamblado.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
