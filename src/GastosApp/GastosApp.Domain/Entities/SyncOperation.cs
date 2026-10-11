using GastosApp.Domain.Common;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;

namespace GastosApp.Domain.Entities;

/// <summary>
/// Operacion en la cola de sincronizacion offline del dispositivo.
/// Cada operacion tiene un identificador idempotente generado en el cliente (Id = Guid del cliente).
/// </summary>
public sealed class SyncOperation : Entity
{
    public Guid UserId { get; private set; }
    public SyncOperationType OperationType { get; private set; }
    public SyncOperationStatus Status { get; private set; }

    /// <summary>Payload serializado en JSON con los datos de la operacion.</summary>
    public string Payload { get; private set; } = string.Empty;

    /// <summary>Timestamp del dispositivo cuando se creo la operacion.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Ultima vez que se intento sincronizar.</summary>
    public DateTimeOffset? LastAttemptAt { get; private set; }

    /// <summary>Contador de intentos fallidos.</summary>
    public int AttemptCount { get; private set; }

    /// <summary>Mensaje del ultimo error o conflicto.</summary>
    public string? ErrorMessage { get; private set; }

    private SyncOperation() { }

    public static SyncOperation Create(Guid id, Guid userId, SyncOperationType operationType, string payload)
    {
        if (userId == Guid.Empty)
            throw new DomainException("El usuario es obligatorio para crear una operacion de sincronizacion.");

        if (string.IsNullOrWhiteSpace(payload))
            throw new DomainException("El payload de la operacion no puede estar vacio.");

        return new SyncOperation
        {
            Id = id,
            UserId = userId,
            OperationType = operationType,
            Status = SyncOperationStatus.Pending,
            Payload = payload,
            CreatedAt = DateTimeOffset.UtcNow,
            AttemptCount = 0
        };
    }

    public void MarkSyncing()
    {
        if (Status == SyncOperationStatus.Synced)
            throw new DomainException("La operacion ya fue sincronizada.");

        Status = SyncOperationStatus.Syncing;
        LastAttemptAt = DateTimeOffset.UtcNow;
        AttemptCount++;
    }

    public void MarkSynced()
    {
        Status = SyncOperationStatus.Synced;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = SyncOperationStatus.Failed;
        ErrorMessage = errorMessage;
    }

    public void MarkConflict(string conflictDetails)
    {
        Status = SyncOperationStatus.Conflict;
        ErrorMessage = conflictDetails;
    }

    public void ResetForRetry()
    {
        if (Status == SyncOperationStatus.Synced)
            throw new DomainException("No se puede reintentar una operacion ya sincronizada.");

        Status = SyncOperationStatus.Pending;
        ErrorMessage = null;
    }
}
