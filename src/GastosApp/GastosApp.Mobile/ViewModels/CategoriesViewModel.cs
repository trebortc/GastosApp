using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GastosApp.Application.Categories;
using GastosApp.Mobile.Services;
using System.Collections.ObjectModel;

namespace GastosApp.Mobile.ViewModels;

public sealed partial class CategoriesViewModel : ObservableObject
{
    private readonly ICategoryService _service;

    [ObservableProperty] private bool    _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<CategoryDto> SystemCategories   { get; } = [];
    public ObservableCollection<CategoryDto> PersonalCategories { get; } = [];

    public CategoriesViewModel(ICategoryService service)
    {
        _service = service;
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var all = await _service.GetAllAsync(ct);

            SystemCategories.Clear();
            PersonalCategories.Clear();

            foreach (var c in all)
            {
                if (c.IsSystem)
                    SystemCategories.Add(c);
                else
                    PersonalCategories.Add(c);
            }
        }
        catch (Exception)
        {
            ErrorMessage = "No se pudieron cargar las categorias.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task AddAsync()
        => await Shell.Current.GoToAsync("categorydetail");

    [RelayCommand]
    private static async Task EditAsync(CategoryDto category)
        => await Shell.Current.GoToAsync($"categorydetail?id={category.Id}");

    [RelayCommand]
    private async Task DeleteAsync(CategoryDto category, CancellationToken ct)
    {
        bool confirmed = await Shell.Current.DisplayAlert(
            "Eliminar categoria",
            $"Deseas eliminar \"{category.Name}\"?",
            "Eliminar", "Cancelar");

        if (!confirmed) return;

        try
        {
            await _service.DeleteAsync(category.Id, ct);
            PersonalCategories.Remove(category);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            await Shell.Current.DisplayAlert("Sin permiso",
                "Las categorias del sistema no se pueden eliminar.", "OK");
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo eliminar la categoria.", "OK");
        }
    }
}
