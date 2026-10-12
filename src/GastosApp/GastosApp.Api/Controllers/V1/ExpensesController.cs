using GastosApp.Api.Extensions;
using GastosApp.Application.Common;
using GastosApp.Application.Expenses;
using GastosApp.Domain.Entities;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GastosApp.Api.Controllers.V1;

[ApiController]
[Route("api/v1/expenses")]
[Authorize]
public sealed class ExpensesController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public ExpensesController(IApplicationDbContext db) => _db = db;

    /// <summary>
    /// Lista gastos personales del usuario autenticado con filtros y paginacion.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ExpenseDto>>> GetPersonal(
        [FromQuery] ExpenseFilter filter, CancellationToken ct)
    {
        var userId = User.GetUserId();

        var query = _db.Expenses
            .Where(e => e.PersonalOwnerUserId == userId);

        query = ApplyFilters(query, filter);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => ToDto(e))
            .ToListAsync(ct);

        return Ok(new PagedResult<ExpenseDto>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    /// <summary>Lista gastos de un grupo con filtros y paginacion.</summary>
    [HttpGet("groups/{groupId:guid}")]
    public async Task<ActionResult<PagedResult<ExpenseDto>>> GetByGroup(
        Guid groupId, [FromQuery] ExpenseFilter filter, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await EnsureGroupMemberAsync(groupId, userId, ct);

        var query = _db.Expenses.Where(e => e.GroupId == groupId);
        query = ApplyFilters(query, filter);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => ToDto(e))
            .ToListAsync(ct);

        return Ok(new PagedResult<ExpenseDto>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    /// <summary>Obtiene un gasto por Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id, ct);

        if (expense is null) return NotFound();
        await EnsureCanReadExpenseAsync(expense, userId, ct);

        return Ok(ToDto(expense));
    }

    /// <summary>Crea un nuevo gasto o ingreso (personal o de grupo).</summary>
    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(
        [FromBody] CreateExpenseRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();

        // Validar que la categoria pertenece al usuario o es del sistema
        var categoryExists = await _db.Categories.AnyAsync(
            c => c.Id == request.CategoryId &&
                 (c.OwnerUserId == null || c.OwnerUserId == userId), ct);

        if (!categoryExists)
            return BadRequest(new { error = "Categoria no valida para este usuario." });

        // Validar pertenencia al grupo si aplica
        if (request.GroupId.HasValue)
            await EnsureGroupMemberAsync(request.GroupId.Value, userId, ct);

        var expense = Expense.Create(
            Guid.NewGuid(),
            request.GroupId.HasValue ? null : userId,
            request.GroupId,
            request.CategoryId,
            userId,
            request.Type,
            request.Amount,
            request.Currency,
            request.Description,
            request.OccurredAt);

        await _db.Expenses.AddAsync(expense, ct);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, ToDto(expense));
    }

    /// <summary>Actualiza un gasto existente.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> Update(
        Guid id, [FromBody] UpdateExpenseRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id, ct);

        if (expense is null) return NotFound();

        var categoryExists = await _db.Categories.AnyAsync(
            c => c.Id == request.CategoryId &&
                 (c.OwnerUserId == null || c.OwnerUserId == userId), ct);

        if (!categoryExists)
            return BadRequest(new { error = "Categoria no valida para este usuario." });

        expense.Update(request.CategoryId, request.Type, request.Amount,
            request.Currency, request.Description, request.OccurredAt, userId);

        await _db.SaveChangesAsync(ct);
        return Ok(ToDto(expense));
    }

    /// <summary>Eliminacion logica de un gasto.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id, ct);

        if (expense is null) return NotFound();

        expense.Delete(userId);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // -- Helpers --

    private static IQueryable<Expense> ApplyFilters(IQueryable<Expense> query, ExpenseFilter filter)
    {
        if (filter.From.HasValue)    query = query.Where(e => e.OccurredAt >= filter.From.Value);
        if (filter.To.HasValue)      query = query.Where(e => e.OccurredAt <= filter.To.Value);
        if (filter.CategoryId.HasValue) query = query.Where(e => e.CategoryId == filter.CategoryId.Value);
        if (filter.Type.HasValue)    query = query.Where(e => e.Type == filter.Type.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(e => e.Description.Contains(filter.Search));
        return query;
    }

    private async Task EnsureGroupMemberAsync(Guid groupId, Guid userId, CancellationToken ct)
    {
        var isMember = await _db.GroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId, ct);

        if (!isMember)
            throw new ForbiddenException("No eres miembro de este grupo.");
    }

    private async Task EnsureCanReadExpenseAsync(Expense expense, Guid userId, CancellationToken ct)
    {
        if (expense.IsPersonal && expense.PersonalOwnerUserId != userId)
            throw new ForbiddenException("No tienes acceso a este movimiento.");

        if (expense.IsGroupExpense)
            await EnsureGroupMemberAsync(expense.GroupId!.Value, userId, ct);
    }

    private static ExpenseDto ToDto(Expense e) => new(
        e.Id,
        e.PersonalOwnerUserId,
        e.GroupId,
        e.CategoryId,
        string.Empty, // El nombre de categoria se resuelve en consultas con Join si se necesita
        e.CreatedByUserId,
        e.Type,
        e.Money.Amount,
        e.Money.Currency,
        e.Description,
        e.OccurredAt,
        e.CreatedAt,
        e.UpdatedAt);
}
