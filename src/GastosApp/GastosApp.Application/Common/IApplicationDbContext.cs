using GastosApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GastosApp.Application.Common;

/// <summary>
/// Contrato del contexto de base de datos expuesto hacia la capa de Application.
/// Permite que los casos de uso accedan a los DbSets sin depender de EF Core ni de Infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Category> Categories { get; }
    DbSet<ExpenseGroup> ExpenseGroups { get; }
    DbSet<GroupMember> GroupMembers { get; }
    DbSet<GroupInvitation> GroupInvitations { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<Budget> Budgets { get; }
    DbSet<SyncOperation> SyncOperations { get; }
    DbSet<ProcessedOperation> ProcessedOperations { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
