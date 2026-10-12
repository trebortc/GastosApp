using System.ComponentModel.DataAnnotations;

namespace GastosApp.Application.Budgets;

public sealed record CreateBudgetRequest(
    [Required, MaxLength(150)] string Name,
    [Required, Range(0.01, double.MaxValue)] decimal LimitAmount,
    [Required, StringLength(3, MinimumLength = 3)] string Currency,
    [Required] DateTimeOffset PeriodStart,
    [Required] DateTimeOffset PeriodEnd,
    /// <summary>Nulo si es presupuesto personal. Requerido si es de grupo.</summary>
    Guid? GroupId = null);

public sealed record UpdateBudgetRequest(
    [Required, MaxLength(150)] string Name,
    [Required, Range(0.01, double.MaxValue)] decimal LimitAmount);

public sealed record BudgetDto(
    Guid Id,
    string Name,
    Guid? OwnerUserId,
    Guid? GroupId,
    decimal LimitAmount,
    string Currency,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    bool IsActive,
    /// <summary>Total gastado en el periodo (calculado en la consulta).</summary>
    decimal SpentAmount,
    decimal RemainingAmount,
    double PercentUsed);
