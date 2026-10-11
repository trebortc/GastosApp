namespace GastosApp.Domain.Common;

/// <summary>
/// Clase base para todas las entidades del dominio.
/// Usa Guid generado en cliente para soportar creación offline.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }

    protected Entity() { }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador de la entidad no puede ser vacío.", nameof(id));
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
