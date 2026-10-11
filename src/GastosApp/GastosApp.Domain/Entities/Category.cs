using GastosApp.Domain.Common;
using GastosApp.Domain.Exceptions;

namespace GastosApp.Domain.Entities;

/// <summary>
/// Categoría de un movimiento. Puede ser personal (del usuario) o del sistema (compartida).
/// Las categorías del sistema tienen OwnerUserId nulo.
/// </summary>
public sealed class Category : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public string? Color { get; private set; }

    /// <summary>
    /// Nulo para categorías predeterminadas del sistema,
    /// o el Guid del usuario propietario para categorías personales.
    /// </summary>
    public Guid? OwnerUserId { get; private set; }

    public bool IsSystem => OwnerUserId is null;
    public bool IsDeleted { get; private set; }

    private Category() { }

    public static Category CreatePersonal(Guid id, Guid ownerUserId, string name, string? icon = null, string? color = null)
    {
        if (ownerUserId == Guid.Empty)
            throw new DomainException("El propietario de la categoría es obligatorio.");

        return Build(id, ownerUserId, ownerUserId, name, icon, color);
    }

    public static Category CreateSystem(Guid id, string name, string? icon = null, string? color = null)
    {
        return Build(id, null, Guid.Empty, name, icon, color);
    }

    private static Category Build(Guid id, Guid? ownerUserId, Guid createdBy, string name, string? icon, string? color)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre de la categoría es obligatorio.");

        return new Category
        {
            Id = id,
            OwnerUserId = ownerUserId,
            CreatedByUserId = createdBy,
            Name = name.Trim(),
            Icon = icon?.Trim(),
            Color = color?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Rename(string newName)
    {
        if (IsSystem)
            throw new ForbiddenException("Las categorías del sistema no pueden modificarse.");

        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("El nuevo nombre de la categoría es obligatorio.");

        Name = newName.Trim();
        SetUpdated();
    }

    public void Delete()
    {
        if (IsSystem)
            throw new ForbiddenException("Las categorías del sistema no pueden eliminarse.");

        IsDeleted = true;
        SetUpdated();
    }
}
