using System.Net.Http.Json;
using GastosApp.Application.Categories;

namespace GastosApp.Mobile.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly HttpClient _http;

    public CategoryService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("GastosApi");
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<IReadOnlyList<CategoryDto>>("api/v1/categories", ct);
        return result ?? [];
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/v1/categories", request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryDto>(cancellationToken: ct))!;
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/categories/{id}", request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryDto>(cancellationToken: ct))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/v1/categories/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}
