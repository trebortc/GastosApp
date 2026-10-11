using GastosApp.Application.Common;
using GastosApp.Domain.Entities;
using GastosApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GastosApp.Infrastructure.Persistence;

/// <summary>
/// Contexto principal de Entity Framework Core conectado a PostgreSQL.
/// Hereda de IdentityDbContext para integrar ASP.NET Core Identity.
/// La cadena de conexion se inyecta desde variables de entorno o secretos del proveedor.
/// Nunca guardes credenciales reales en appsettings.json ni en el repositorio.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // -- Dominio --
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ExpenseGroup> ExpenseGroups => Set<ExpenseGroup>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupInvitation> GroupInvitations => Set<GroupInvitation>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<SyncOperation> SyncOperations => Set<SyncOperation>();
    public DbSet<ProcessedOperation> ProcessedOperations => Set<ProcessedOperation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // -- Identity extendido --
    public DbSet<ApplicationRefreshToken> RefreshTokens => Set<ApplicationRefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automaticamente todas las IEntityTypeConfiguration<T> del ensamblado.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
