using GastosApp.Domain.Enums;

namespace GastosApp.Application.Reports;

// -- Filtro comun de reporte --

public sealed record ReportFilter(
    DateTimeOffset From,
    DateTimeOffset To,
    Guid? GroupId = null,
    string? Currency = null);

// -- Resumen del panel financiero --

public sealed record DashboardDto(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetBalance,
    string Currency,
    DateTimeOffset From,
    DateTimeOffset To,
    IReadOnlyList<CategorySummaryDto> ByCategory,
    IReadOnlyList<MonthlySummaryDto> Monthly);

public sealed record CategorySummaryDto(
    Guid CategoryId,
    string CategoryName,
    decimal Amount,
    double Percentage);

public sealed record MonthlySummaryDto(
    int Year,
    int Month,
    decimal Income,
    decimal Expenses,
    decimal Net);

// -- Reporte detallado --

public sealed record PeriodComparisonDto(
    PeriodTotalsDto Current,
    PeriodTotalsDto Previous,
    decimal IncomeDelta,
    decimal ExpensesDelta);

public sealed record PeriodTotalsDto(
    DateTimeOffset From,
    DateTimeOffset To,
    decimal Income,
    decimal Expenses,
    decimal Net);
