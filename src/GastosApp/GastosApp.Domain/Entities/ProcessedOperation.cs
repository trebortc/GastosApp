using GastosApp.Domain.Common;

namespace GastosApp.Domain.Entities;

/// <summary>
/// Registro de una operacion que el servidor ya proceso.
/// Garantiza idempotencia: si el cliente reintenta enviar la misma operacion
/// (mismo Id), el servidor la detecta como duplicada y no la procesa dos veces.
/// </summary>
public sealed class ProcessedOperation : Entity
{
    /// <summary>Id identico al SyncOperation.Id enviado por el cliente.</summary>
    public Guid OperationId => Id;

    public Guid UserId { get; private set; }
    public DateTimeOffset ProcessedAt { get; private set; }

    private ProcessedOperation() { }

    public static ProcessedOperation Create(Guid operationId, Guid userId)
    {
        return new ProcessedOperation
        {
            Id = operationId,
            UserId = userId,
            ProcessedAt = DateTimeOffset.UtcNow
        };
    }
}
