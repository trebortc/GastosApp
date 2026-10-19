using System.Net.Http.Json;
using System.Web;
using GastosApp.Application.Categories;
using GastosApp.Application.Common;
using GastosApp.Application.Expenses;

namespace GastosApp.Mobile.Services;

public sealed class ExpenseService : IExpenseService
{
    private readonly HttpClient _http;

    public ExpenseService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("GastosApi");
    }

    public async Task<PagedResult<ExpenseDto>> GetPersonalAsync(
        ExpenseFilter filter, CancellationToken ct = default)
    {
        var q = HttpUtility.ParseQueryString(string.Empty);
        if (filter.From.HasValue)      q["from"]       = filter.From.Value.ToString("O");
        if (filter.To.HasValue)        q["to"]         = filter.To.Value.ToString("O");
        if (filter.CategoryId.HasValue) q["categoryId"] = filter.CategoryId.Value.ToString();
        if (filter.Search is not null)  q["search"]     = filter.Search;
        q["page"]     = filter.Page.ToString();
        q["pageSize"] = filter.PageSize.ToString();

        var url = $"api/v1/expenses?{q}";
        var result = await _http.GetFromJsonAsync<PagedResult<ExpenseDto>>(url, ct);
        return result ?? new PagedResult<ExpenseDto> { Items = [], TotalCount = 0, Page = filter.Page, PageSize = filter.PageSize };
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/v1/expenses", request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExpenseDto>(cancellationToken: ct))!;
    }

    public async Task UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/expenses/{id}", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/v1/expenses/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<PagedResult<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<PagedResult<CategoryDto>>("api/v1/expenses/categories", ct);
        return result ?? new PagedResult<CategoryDto>
        {
            Items = [],
            TotalCount = 0,
            Page = 1,
            PageSize = 100
        };
    }
}
