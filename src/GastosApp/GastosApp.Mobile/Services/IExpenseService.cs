using GastosApp.Application.Common;
using GastosApp.Application.Expenses;

namespace GastosApp.Mobile.Services;

public interface IExpenseService
{
    Task<PagedResult<ExpenseDto>> GetPersonalAsync(ExpenseFilter filter, CancellationToken ct = default);
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
