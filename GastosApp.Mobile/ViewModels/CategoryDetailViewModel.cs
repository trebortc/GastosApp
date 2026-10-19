using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GastosApp.Application.Categories;
using GastosApp.Mobile.Services;

namespace GastosApp.Mobile.ViewModels;

[QueryProperty(nameof(CategoryId), "id")]
public sealed partial class CategoryDetailViewModel : ObservableObject
{
    private readonly ICategoryService _service;

    [ObservableProperty] private string?  _categoryId;
    [ObservableProperty] private bool     _isBusy;
    [ObservableProperty] private string?  _errorMessage;
    [ObservableProperty] private bool     _isEditMode;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;

    [ObservableProperty] private string? _icon;
    [ObservableProperty] private string? _color;

    // Paleta de colores predefinidos para que el usuario elija
    public IReadOnlyList<string> ColorPalette { get; } =
    [
        "#EF4444", "#F97316", "#EAB308", "#22C55E",
        "#14B8A6", "#3B82F6", "#8B5CF6", "#EC4899",
        "#64748B", "#0EA5E9", "#10B981", "#F59E0B"
    ];

    // Iconos / emojis comunes para categorias
    public IReadOnlyList<string> IconOptions { get; } =
    [
        "🛒", "🍔", "🚗", "🏠", "💊", "🎉",
        "✈️", "📚", "💻", "👕", "💰", "🎬",
        "🏋️", "☕", "🐾", "🎮", "🏥", "⚡"
    ];

    public CategoryDetailViewModel(ICategoryService service)
    {
        _service = service;
    }

    partial void OnCategoryIdChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            IsEditMode = true;
            LoadCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(CategoryId)) return;
        IsBusy = true;
        try
        {
            // Cargamos la lista y filtramos por Id (no hay endpoint individual)
            var all = await _service.GetAllAsync(ct);
            var cat = all.FirstOrDefault(c => c.Id.ToString() == CategoryId);
            if (cat is null) return;

            Name  = cat.Name;
            Icon  = cat.Icon;
            Color = cat.Color;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave => !string.IsNullOrWhiteSpace(Name) && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync(CancellationToken ct)
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            if (IsEditMode && Guid.TryParse(CategoryId, out var id))
            {
                await _service.UpdateAsync(id, new UpdateCategoryRequest(Name.Trim(), Icon, Color), ct);
            }
            else
            {
                await _service.CreateAsync(new CreateCategoryRequest(Name.Trim(), Icon, Color), ct);
            }
            await Shell.Current.GoToAsync("..");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            ErrorMessage = "Ya existe una categoria con ese nombre.";
        }
        catch (Exception)
        {
            ErrorMessage = "No se pudo guardar la categoria.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task CancelAsync()
        => await Shell.Current.GoToAsync("..");
}
