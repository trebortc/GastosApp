using GastosApp.Api.Extensions;
using GastosApp.Application.Common;
using GastosApp.Application.Reports;
using GastosApp.Domain.Enums;
using GastosApp.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GastosApp.Api.Controllers.V1;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public ReportsController(IApplicationDbContext db) => _db = db;

    /// <summary>
    /// Panel financiero: totales, distribucion por categoria y resumen mensual.
    /// Si se pasa groupId, muestra los datos del grupo (el usuario debe ser miembro).
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard(
        [FromQuery] ReportFilter filter, CancellationToken ct)
    {
        if (filter.From >= filter.To)
            return BadRequest(new { error = "La fecha de inicio debe ser anterior a la de fin." });

        var userId = User.GetUserId();
        var currency = filter.Currency ?? "MXN";

        if (filter.GroupId.HasValue)
            await EnsureGroupMemberAsync(filter.GroupId.Value, userId, ct);

        var expensesQuery = BuildBaseQuery(filter, userId, currency);

        var totalIncome = await expensesQuery
            .Where(e => e.Type == ExpenseType.Income)
            .SumAsync(e => e.Money.Amount, ct);

        var totalExpenses = await expensesQuery
            .Where(e => e.Type == ExpenseType.Expense)
            .SumAsync(e => e.Money.Amount, ct);

        // Distribucion por categoria
        var byCategory = await expensesQuery
            .Where(e => e.Type == ExpenseType.Expense)
            .GroupBy(e => new { e.CategoryId })
            .Select(g => new
            {
                g.Key.CategoryId,
                Amount = g.Sum(e => e.Money.Amount)
            })
            .ToListAsync(ct);

        var categoryIds = byCategory.Select(c => c.CategoryId).ToList();
        var categoryNames = await _db.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var categorySummary = byCategory
            .Select(c => new CategorySummaryDto(
                c.CategoryId,
                categoryNames.TryGetValue(c.CategoryId, out var n) ? n : "Sin categoria",
                c.Amount,
                totalExpenses > 0 ? Math.Round((double)(c.Amount / totalExpenses) * 100, 1) : 0))
            .OrderByDescending(c => c.Amount)
            .ToList();

        // Resumen mensual
        var monthly = await expensesQuery
            .GroupBy(e => new
            {
                Year = e.OccurredAt.Year,
                Month = e.OccurredAt.Month
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Income = g.Where(e => e.Type == ExpenseType.Income).Sum(e => e.Money.Amount),
                Expenses = g.Where(e => e.Type == ExpenseType.Expense).Sum(e => e.Money.Amount)
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .Select(m => new MonthlySummaryDto(m.Year, m.Month, m.Income, m.Expenses, m.Income - m.Expenses))
            .ToListAsync(ct);

        return Ok(new DashboardDto(
            totalIncome, totalExpenses, totalIncome - totalExpenses,
            currency, filter.From, filter.To, categorySummary, monthly));
    }

    /// <summary>Comparacion entre el periodo actual y el anterior de igual duracion.</summary>
    [HttpGet("comparison")]
    public async Task<ActionResult<PeriodComparisonDto>> Comparison(
        [FromQuery] ReportFilter filter, CancellationToken ct)
    {
        if (filter.From >= filter.To)
            return BadRequest(new { error = "La fecha de inicio debe ser anterior a la de fin." });

        var userId = User.GetUserId();
        var currency = filter.Currency ?? "MXN";

        if (filter.GroupId.HasValue)
            await EnsureGroupMemberAsync(filter.GroupId.Value, userId, ct);

        var duration = filter.To - filter.From;
        var prevFrom = filter.From - duration;
        var prevTo = filter.From;

        var currentTotals = await GetPeriodTotalsAsync(filter with { }, userId, currency, ct);
        var previousFilter = filter with { From = prevFrom, To = prevTo };
        var previousTotals = await GetPeriodTotalsAsync(previousFilter, userId, currency, ct);

        return Ok(new PeriodComparisonDto(
            currentTotals, previousTotals,
            currentTotals.Income - previousTotals.Income,
            currentTotals.Expenses - previousTotals.Expenses));
    }

    // -- Helpers --

    private IQueryable<Domain.Entities.Expense> BuildBaseQuery(
        ReportFilter filter, Guid userId, string currency)
    {
        var q = _db.Expenses
            .Where(e => e.OccurredAt >= filter.From && e.OccurredAt <= filter.To
                        && e.Money.Currency == currency);

        if (filter.GroupId.HasValue)
            q = q.Where(e => e.GroupId == filter.GroupId.Value);
        else
            q = q.Where(e => e.PersonalOwnerUserId == userId);

        return q;
    }

    private async Task<PeriodTotalsDto> GetPeriodTotalsAsync(
        ReportFilter filter, Guid userId, string currency, CancellationToken ct)
    {
        var q = BuildBaseQuery(filter, userId, currency);

        var income   = await q.Where(e => e.Type == ExpenseType.Income).SumAsync(e => e.Money.Amount, ct);
        var expenses = await q.Where(e => e.Type == ExpenseType.Expense).SumAsync(e => e.Money.Amount, ct);

        return new PeriodTotalsDto(filter.From, filter.To, income, expenses, income - expenses);
    }

    private async Task EnsureGroupMemberAsync(Guid groupId, Guid userId, CancellationToken ct)
    {
        var isMember = await _db.GroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId, ct);
        if (!isMember)
            throw new ForbiddenException("No eres miembro de este grupo.");
    }
}
