using GastosApp.Api.Extensions;
using GastosApp.Application.Common;
using GastosApp.Application.Groups;
using GastosApp.Domain.Entities;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;
using GastosApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GastosApp.Api.Controllers.V1;

[ApiController]
[Route("api/v1/groups")]
[Authorize]
public sealed class GroupsController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public GroupsController(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    /// <summary>Lista los grupos a los que pertenece el usuario autenticado.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GroupDto>>> GetMyGroups(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var groups = await _db.ExpenseGroups
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .Select(g => new GroupDto(
                g.Id, g.Name, g.Description, g.DefaultCurrency,
                g.IsActive, g.CreatedByUserId, g.CreatedAt,
                g.Members.Count))
            .ToListAsync(ct);

        return Ok(groups);
    }

    /// <summary>Obtiene el detalle de un grupo con sus miembros.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GroupDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var group = await _db.ExpenseGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (group is null) return NotFound();
        EnsureIsMember(group, userId);

        var memberUserIds = group.Members.Select(m => m.UserId).ToList();
        var users = await _userManager.Users
            .Where(u => memberUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        var memberDtos = group.Members.Select(m =>
        {
            users.TryGetValue(m.UserId, out var u);
            return new GroupMemberDto(m.Id, m.UserId,
                u?.DisplayName ?? "Usuario", u?.Email ?? string.Empty,
                m.Role, m.JoinedAt);
        }).ToList();

        return Ok(new GroupDetailDto(group.Id, group.Name, group.Description,
            group.DefaultCurrency, group.IsActive, group.CreatedByUserId,
            group.CreatedAt, memberDtos));
    }

    /// <summary>Crea un nuevo grupo. El usuario autenticado pasa a ser Owner.</summary>
    [HttpPost]
    public async Task<ActionResult<GroupDto>> Create(
        [FromBody] CreateGroupRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();
        var group = ExpenseGroup.Create(Guid.NewGuid(), userId,
            request.Name, request.Description, request.DefaultCurrency);

        await _db.ExpenseGroups.AddAsync(group, ct);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = group.Id },
            new GroupDto(group.Id, group.Name, group.Description,
                group.DefaultCurrency, group.IsActive, group.CreatedByUserId,
                group.CreatedAt, group.Members.Count));
    }

    /// <summary>Actualiza el nombre o descripcion del grupo.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateGroupRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();
        var group = await _db.ExpenseGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (group is null) return NotFound();

        group.Rename(request.Name, userId);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Desactiva el grupo (solo el Owner puede hacerlo).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var group = await _db.ExpenseGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (group is null) return NotFound();

        group.Deactivate(userId);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- Miembros ----

    /// <summary>Cambia el rol de un miembro del grupo.</summary>
    [HttpPut("{id:guid}/members/{memberUserId:guid}/role")]
    public async Task<IActionResult> UpdateMemberRole(
        Guid id, Guid memberUserId,
        [FromBody] UpdateMemberRoleRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();
        var group = await _db.ExpenseGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (group is null) return NotFound();

        var member = group.Members.FirstOrDefault(m => m.UserId == memberUserId);
        if (member is null) return NotFound();

        member.ChangeRole(request.Role, userId);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Elimina a un miembro del grupo.</summary>
    [HttpDelete("{id:guid}/members/{memberUserId:guid}")]
    public async Task<IActionResult> RemoveMember(
        Guid id, Guid memberUserId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var group = await _db.ExpenseGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (group is null) return NotFound();

        group.RemoveMember(memberUserId, userId);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- Invitaciones ----

    /// <summary>Envia una invitacion por correo a un nuevo miembro.</summary>
    [HttpPost("{id:guid}/invitations")]
    public async Task<ActionResult<InvitationDto>> CreateInvitation(
        Guid id, [FromBody] CreateInvitationRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();
        var group = await _db.ExpenseGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (group is null) return NotFound();
        EnsureIsAdminOrOwner(group, userId);

        var alreadyMember = await _userManager.Users
            .AnyAsync(u => u.Email == request.InviteeEmail &&
                group.Members.Select(m => m.UserId).Contains(u.Id), ct);

        if (alreadyMember)
            return Conflict(new { error = "El usuario ya es miembro del grupo." });

        var invitation = GroupInvitation.Create(
            Guid.NewGuid(), id, userId,
            request.InviteeEmail, request.Role, request.ExpirationHours);

        await _db.GroupInvitations.AddAsync(invitation, ct);
        await _db.SaveChangesAsync(ct);

        return StatusCode(201, new InvitationDto(
            invitation.Id, invitation.GroupId, group.Name,
            invitation.InviteeEmail, invitation.AssignedRole,
            invitation.Status, invitation.CreatedAt, invitation.ExpiresAt));
    }

    /// <summary>Acepta una invitacion. Solo el destinatario puede aceptarla.</summary>
    [HttpPost("invitations/{invitationId:guid}/accept")]
    public async Task<IActionResult> AcceptInvitation(Guid invitationId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Unauthorized();

        var invitation = await _db.GroupInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId &&
                i.InviteeEmail == user.Email, ct);

        if (invitation is null) return NotFound();

        invitation.Accept(userId);

        var group = await _db.ExpenseGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == invitation.GroupId, ct);

        if (group is null) return NotFound();

        group.AddMember(userId, group.CreatedByUserId, invitation.AssignedRole);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Rechaza una invitacion.</summary>
    [HttpPost("invitations/{invitationId:guid}/decline")]
    public async Task<IActionResult> DeclineInvitation(Guid invitationId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Unauthorized();

        var invitation = await _db.GroupInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId &&
                i.InviteeEmail == user.Email, ct);

        if (invitation is null) return NotFound();

        invitation.Decline();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Lista las invitaciones pendientes del usuario autenticado.</summary>
    [HttpGet("invitations/pending")]
    public async Task<ActionResult<IReadOnlyList<InvitationDto>>> GetPendingInvitations(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Unauthorized();

        var invitations = await _db.GroupInvitations
            .Where(i => i.InviteeEmail == user.Email &&
                        i.Status == Domain.Enums.GroupInvitationStatus.Pending)
            .Join(_db.ExpenseGroups,
                i => i.GroupId,
                g => g.Id,
                (i, g) => new InvitationDto(
                    i.Id, i.GroupId, g.Name, i.InviteeEmail,
                    i.AssignedRole, i.Status, i.CreatedAt, i.ExpiresAt))
            .ToListAsync(ct);

        return Ok(invitations);
    }

    // -- Helpers --

    private static void EnsureIsMember(ExpenseGroup group, Guid userId)
    {
        if (!group.Members.Any(m => m.UserId == userId))
            throw new ForbiddenException("No eres miembro de este grupo.");
    }

    private static void EnsureIsAdminOrOwner(ExpenseGroup group, Guid userId)
    {
        var member = group.Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null ||
            (member.Role != GroupMemberRole.Owner && member.Role != GroupMemberRole.Admin))
            throw new ForbiddenException("Se requiere rol de administrador o propietario.");
    }
}
