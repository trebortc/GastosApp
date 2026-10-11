using GastosApp.Domain.Common;
using GastosApp.Domain.Exceptions;

namespace GastosApp.Domain.Entities;

/// <summary>
/// Grupo de gastos compartidos. El propietario es quien lo crea.
/// </summary>
public sealed class ExpenseGroup : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string DefaultCurrency { get; private set; } = "MXN";
    public bool IsActive { get; private set; } = true;

    private readonly List<GroupMember> _members = [];
    public IReadOnlyList<GroupMember> Members => _members.AsReadOnly();

    private ExpenseGroup() { }

    public static ExpenseGroup Create(Guid id, Guid ownerUserId, string name,
        string? description = null, string defaultCurrency = "MXN")
    {
        if (ownerUserId == Guid.Empty)
            throw new DomainException("El propietario del grupo es obligatorio.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del grupo es obligatorio.");

        if (string.IsNullOrWhiteSpace(defaultCurrency) || defaultCurrency.Length != 3)
            throw new DomainException("La moneda debe ser un código ISO 4217 de tres letras.");

        var now = DateTimeOffset.UtcNow;
        var group = new ExpenseGroup
        {
            Id = id,
            CreatedByUserId = ownerUserId,
            Name = name.Trim(),
            Description = description?.Trim(),
            DefaultCurrency = defaultCurrency.ToUpperInvariant(),
            CreatedAt = now,
            UpdatedAt = now
        };

        // El creador es automáticamente Owner.
        group._members.Add(GroupMember.CreateOwner(Guid.NewGuid(), id, ownerUserId));

        return group;
    }

    public void Rename(string newName, Guid requestingUserId)
    {
        EnsureIsAdmin(requestingUserId);

        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("El nuevo nombre del grupo es obligatorio.");

        Name = newName.Trim();
        SetUpdated();
    }

    public GroupMember AddMember(Guid newMemberId, Guid requestingUserId,
        Enums.GroupMemberRole role = Enums.GroupMemberRole.Member)
    {
        EnsureIsAdmin(requestingUserId);

        if (_members.Any(m => m.UserId == newMemberId))
            throw new DomainException("El usuario ya es miembro del grupo.");

        var member = GroupMember.Create(Guid.NewGuid(), Id, newMemberId, role);
        _members.Add(member);
        SetUpdated();
        return member;
    }

    public void RemoveMember(Guid memberUserId, Guid requestingUserId)
    {
        EnsureIsAdmin(requestingUserId);

        var member = _members.FirstOrDefault(m => m.UserId == memberUserId)
            ?? throw new NotFoundException(nameof(GroupMember), memberUserId);

        if (member.Role == Enums.GroupMemberRole.Owner)
            throw new DomainException("No se puede eliminar al propietario del grupo.");

        _members.Remove(member);
        SetUpdated();
    }

    public void Deactivate(Guid requestingUserId)
    {
        EnsureIsOwner(requestingUserId);
        IsActive = false;
        SetUpdated();
    }

    private void EnsureIsOwner(Guid userId)
    {
        if (!_members.Any(m => m.UserId == userId && m.Role == Enums.GroupMemberRole.Owner))
            throw new ForbiddenException("Solo el propietario puede realizar esta acción.");
    }

    private void EnsureIsAdmin(Guid userId)
    {
        if (!_members.Any(m => m.UserId == userId &&
            (m.Role == Enums.GroupMemberRole.Owner || m.Role == Enums.GroupMemberRole.Admin)))
            throw new ForbiddenException("Se requiere rol de administrador o propietario.");
    }
}
