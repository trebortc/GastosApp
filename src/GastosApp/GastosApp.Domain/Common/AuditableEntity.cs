namespace GastosApp.Domain.Common;

/// <summary>
/// Entidad con campos de auditoría y control de concurrencia optimista.
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset UpdatedAt { get; protected set; }
    public Guid CreatedByUserId { get; protected set; }

    /// <summary>
    /// Token de concurrencia para detección de conflictos.
    /// EF Core lo mapea al campo xmin de PostgreSQL.
    /// </summary>
    public uint Version { get; protected set; }

    protected AuditableEntity() { }

    protected AuditableEntity(Guid id, Guid createdByUserId) : base(id)
    {
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    protected void SetUpdated()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
