using GastosApp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GastosApp.Application.Groups;

// -- Grupos --

public sealed record CreateGroupRequest(
    [Required, MaxLength(150)] string Name,
    [MaxLength(500)] string? Description = null,
    [StringLength(3, MinimumLength = 3)] string DefaultCurrency = "MXN");

public sealed record UpdateGroupRequest(
    [Required, MaxLength(150)] string Name,
    [MaxLength(500)] string? Description = null);

public sealed record GroupDto(
    Guid Id,
    string Name,
    string? Description,
    string DefaultCurrency,
    bool IsActive,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    int MemberCount);

public sealed record GroupDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string DefaultCurrency,
    bool IsActive,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    IReadOnlyList<GroupMemberDto> Members);

// -- Miembros --

public sealed record GroupMemberDto(
    Guid MemberId,
    Guid UserId,
    string DisplayName,
    string Email,
    GroupMemberRole Role,
    DateTimeOffset JoinedAt);

public sealed record UpdateMemberRoleRequest(
    [Required] GroupMemberRole Role);

// -- Invitaciones --

public sealed record CreateInvitationRequest(
    [Required, EmailAddress, MaxLength(256)] string InviteeEmail,
    GroupMemberRole Role = GroupMemberRole.Member,
    int ExpirationHours = 72);

public sealed record InvitationDto(
    Guid Id,
    Guid GroupId,
    string GroupName,
    string InviteeEmail,
    GroupMemberRole AssignedRole,
    GroupInvitationStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);
