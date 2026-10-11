using GastosApp.Domain.Common;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;

namespace GastosApp.Domain.Entities;

/// <summary>Miembro de un grupo de gastos con su rol.</summary>
public sealed class GroupMember : Entity
{
    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
    public GroupMemberRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    private GroupMember() { }

    internal static GroupMember CreateOwner(Guid id, Guid groupId, Guid userId)
        => Create(id, groupId, userId, GroupMemberRole.Owner);

    internal static GroupMember Create(Guid id, Guid groupId, Guid userId, GroupMemberRole role)
    {
        if (groupId == Guid.Empty || userId == Guid.Empty)
            throw new DomainException("El grupo y el usuario son obligatorios para crear un miembro.");

        return new GroupMember
        {
            Id = id,
            GroupId = groupId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTimeOffset.UtcNow
        };
    }

    public void ChangeRole(GroupMemberRole newRole, Guid requestingUserId)
    {
        if (Role == GroupMemberRole.Owner)
            throw new ForbiddenException("El rol del propietario no puede cambiarse.");

        Role = newRole;
    }
}
