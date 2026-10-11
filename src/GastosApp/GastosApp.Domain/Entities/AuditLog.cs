using GastosApp.Domain.Common;

namespace GastosApp.Domain.Entities;

/// <summary>
/// Registro inmutable de eventos importantes del sistema.
/// Nunca almacena contraseñas, tokens ni informacion sensible.
/// </summary>
public sealed class AuditLog : Entity
{
    public Guid? ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }

    /// <summary>Resumen JSON de los cambios. No incluir campos sensibles.</summary>
    public string? ChangeSummary { get; private set; }

    public string? IpAddress { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(Guid id, Guid? actorUserId, string action,
        string entityName, Guid? entityId = null,
        string? changeSummary = null, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("La accion del log es obligatoria.", nameof(action));

        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("El nombre de la entidad es obligatorio.", nameof(entityName));

        return new AuditLog
        {
            Id = id,
            ActorUserId = actorUserId,
            Action = action.Trim(),
            EntityName = entityName.Trim(),
            EntityId = entityId,
            ChangeSummary = changeSummary,
            IpAddress = ipAddress,
            OccurredAt = DateTimeOffset.UtcNow
        };
    }
}
