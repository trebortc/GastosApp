using GastosApp.Domain.Common;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;

namespace GastosApp.Domain.Entities;

/// <summary>
/// Invitación enviada a un usuario para unirse a un grupo.
/// Caduca automáticamente después de un tiempo definido.
/// </summary>
public sealed class GroupInvitation : Entity
{
    public Guid GroupId { get; private set; }

    /// <summary>Usuario que envía la invitación.</summary>
    public Guid InvitedByUserId { get; private set; }

    /// <summary>Email del destinatario (puede no tener cuenta todavía).</summary>
    public string InviteeEmail { get; private set; } = string.Empty;

    /// <summary>UserId del destinatario si ya tiene cuenta, nulo si no.</summary>
    public Guid? InviteeUserId { get; private set; }

    public GroupMemberRole AssignedRole { get; private set; }
    public GroupInvitationStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    private GroupInvitation() { }

    public static GroupInvitation Create(Guid id, Guid groupId, Guid invitedByUserId,
        string inviteeEmail, GroupMemberRole role = GroupMemberRole.Member,
        int expirationHours = 72)
    {
        if (groupId == Guid.Empty || invitedByUserId == Guid.Empty)
            throw new DomainException("El grupo e invitante son obligatorios.");

        if (string.IsNullOrWhiteSpace(inviteeEmail))
            throw new DomainException("El email del destinatario es obligatorio.");

        var now = DateTimeOffset.UtcNow;
        return new GroupInvitation
        {
            Id = id,
            GroupId = groupId,
            InvitedByUserId = invitedByUserId,
            InviteeEmail = inviteeEmail.Trim().ToLowerInvariant(),
            AssignedRole = role,
            Status = GroupInvitationStatus.Pending,
            CreatedAt = now,
            ExpiresAt = now.AddHours(expirationHours)
        };
    }

    public void Accept(Guid inviteeUserId)
    {
        EnsurePending();
        EnsureNotExpired();

        InviteeUserId = inviteeUserId;
        Status = GroupInvitationStatus.Accepted;
    }

    public void Decline()
    {
        EnsurePending();
        Status = GroupInvitationStatus.Declined;
    }

    public void Cancel()
    {
        if (Status != GroupInvitationStatus.Pending)
            throw new DomainException("Solo se pueden cancelar invitaciones pendientes.");

        Status = GroupInvitationStatus.Cancelled;
    }

    public void ExpireIfNeeded()
    {
        if (Status == GroupInvitationStatus.Pending && DateTimeOffset.UtcNow > ExpiresAt)
            Status = GroupInvitationStatus.Expired;
    }

    private void EnsurePending()
    {
        if (Status != GroupInvitationStatus.Pending)
            throw new DomainException($"La invitación no está pendiente. Estado actual: {Status}.");
    }

    private void EnsureNotExpired()
    {
        if (DateTimeOffset.UtcNow > ExpiresAt)
        {
            Status = GroupInvitationStatus.Expired;
            throw new DomainException("La invitación ha expirado.");
        }
    }
}
