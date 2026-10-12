using GastosApp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GastosApp.Application.Expenses;

// -- Requests --

public sealed record CreateExpenseRequest(
    [Required] Guid CategoryId,
    [Required] ExpenseType Type,
    [Required, Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor que cero.")]
    decimal Amount,
    [Required, StringLength(3, MinimumLength = 3)] string Currency,
    [MaxLength(500)] string Description,
    [Required] DateTimeOffset OccurredAt,
    /// <summary>Nulo si es gasto personal. Requerido si es de grupo.</summary>
    Guid? GroupId = null);

public sealed record UpdateExpenseRequest(
    [Required] Guid CategoryId,
    [Required] ExpenseType Type,
    [Required, Range(0.01, double.MaxValue)] decimal Amount,
    [Required, StringLength(3, MinimumLength = 3)] string Currency,
    [MaxLength(500)] string Description,
    [Required] DateTimeOffset OccurredAt);

// -- Responses --

public sealed record ExpenseDto(
    Guid Id,
    Guid? PersonalOwnerUserId,
    Guid? GroupId,
    Guid CategoryId,
    string CategoryName,
    Guid CreatedByUserId,
    ExpenseType Type,
    decimal Amount,
    string Currency,
    string Description,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

// -- Filtros --

public sealed record ExpenseFilter(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    Guid? CategoryId = null,
    ExpenseType? Type = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20);
