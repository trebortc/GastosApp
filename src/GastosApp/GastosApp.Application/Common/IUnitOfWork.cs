namespace GastosApp.Application.Common;

/// <summary>Permite persistir cambios de forma atómica desde los casos de uso.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
