using GastosApp.Api.Extensions;
using GastosApp.Application.Budgets;
using GastosApp.Application.Common;
using GastosApp.Domain.Entities;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GastosApp.Api.Controllers.V1;

[ApiController]
[Route("api/v1/budgets")]
[Authorize]
public sealed class BudgetsController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public BudgetsController(IApplicationDbContext db) => _db = db;

    /// <summary>Lista los presupuestos activos del usuario (personales y de grupos donde es miembro).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BudgetDto>>> GetAll(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var myGroupIds = await _db.GroupMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.GroupId)
            .ToListAsync(ct);

        var budgets = await _db.Budgets
            .Where(b => b.IsActive &&
                (b.OwnerUserId == userId || (b.GroupId != null && myGroupIds.Contains(b.GroupId.Value))))
            .ToListAsync(ct);

        var dtos = new List<BudgetDto>();
        foreach (var b in budgets)
        {
            var spent = await CalculateSpentAsync(b, userId, ct);
            dtos.Add(ToDto(b, spent));
        }

        return Ok(dtos);
    }

    /// <summary>Obtiene un presupuesto por Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BudgetDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var budget = await _db.Budgets.FirstOrDefaultAsync(b => b.Id == id, ct);

        if (budget is null) return NotFound();
        await EnsureCanAccessBudgetAsync(budget, userId, ct);

        var spent = await CalculateSpentAsync(budget, userId, ct);
        return Ok(ToDto(budget, spent));
    }

    /// <summary>Crea un presupuesto personal o de grupo.</summary>
    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Create(
        [FromBody] CreateBudgetRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();

        if (request.GroupId.HasValue)
        {
            var isMember = await _db.GroupMembers
                .AnyAsync(m => m.GroupId == request.GroupId.Value && m.UserId == userId, ct);
            if (!isMember)
                throw new ForbiddenException("No eres miembro de ese grupo.");
        }

        var budget = Budget.Create(
            Guid.NewGuid(), userId,
            request.GroupId.HasValue ? null : userId,
            request.GroupId,
            request.Name, request.LimitAmount, request.Currency,
            request.PeriodStart, request.PeriodEnd);

        await _db.Budgets.AddAsync(budget, ct);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = budget.Id }, ToDto(budget, 0m));
    }

    /// <summary>Actualiza el nombre o limite de un presupuesto.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateBudgetRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();
        var budget = await _db.Budgets.FirstOrDefaultAsync(b => b.Id == id, ct);

        if (budget is null) return NotFound();
        await EnsureCanAccessBudgetAsync(budget, userId, ct);

        budget.UpdateLimit(request.LimitAmount, userId);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Desactiva un presupuesto.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var budget = await _db.Budgets.FirstOrDefaultAsync(b => b.Id == id, ct);

        if (budget is null) return NotFound();
        await EnsureCanAccessBudgetAsync(budget, userId, ct);

        budget.Deactivate(userId);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // -- Helpers --

    private async Task EnsureCanAccessBudgetAsync(Budget budget, Guid userId, CancellationToken ct)
    {
        if (budget.IsPersonal && budget.OwnerUserId != userId)
            throw new ForbiddenException("No tienes acceso a este presupuesto.");

        if (!budget.IsPersonal && budget.GroupId.HasValue)
        {
            var isMember = await _db.GroupMembers
                .AnyAsync(m => m.GroupId == budget.GroupId.Value && m.UserId == userId, ct);
            if (!isMember)
                throw new ForbiddenException("No eres miembro del grupo de este presupuesto.");
        }
    }

    private async Task<decimal> CalculateSpentAsync(Budget budget, Guid userId, CancellationToken ct)
    {
        var query = _db.Expenses
            .Where(e => e.Type == ExpenseType.Expense &&
                        e.OccurredAt >= budget.PeriodStart &&
                        e.OccurredAt <= budget.PeriodEnd &&
                        e.Money.Currency == budget.Currency);

        if (budget.IsPersonal)
            query = query.Where(e => e.PersonalOwnerUserId == userId);
        else
            query = query.Where(e => e.GroupId == budget.GroupId);

        return await query.SumAsync(e => e.Money.Amount, ct);
    }

    private static BudgetDto ToDto(Budget b, decimal spent)
    {
        var remaining = b.LimitAmount - spent;
        var pct = b.LimitAmount > 0 ? (double)(spent / b.LimitAmount) * 100 : 0;
        return new BudgetDto(b.Id, b.Name, b.OwnerUserId, b.GroupId,
            b.LimitAmount, b.Currency, b.PeriodStart, b.PeriodEnd,
            b.IsActive, spent, remaining, Math.Round(pct, 1));
    }
}
