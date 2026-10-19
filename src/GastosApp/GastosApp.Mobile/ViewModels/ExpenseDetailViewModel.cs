using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GastosApp.Application.Categories;
using GastosApp.Application.Expenses;
using GastosApp.Domain.Entities;
using GastosApp.Domain.Enums;
using GastosApp.Mobile.Services;
using System.Collections.ObjectModel;

namespace GastosApp.Mobile.ViewModels;

[QueryProperty(nameof(ExpenseId), "id")]
public sealed partial class ExpenseDetailViewModel : ObservableObject
{
    private readonly IExpenseService _expenses;

    [ObservableProperty] private string?     _expenseId;
    [ObservableProperty] private bool        _isBusy;
    [ObservableProperty] private string?     _errorMessage;
    [ObservableProperty] private bool        _isEditMode;

    // Campos del formulario
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private decimal _amount;

    [ObservableProperty] private string      _currency    = "MXN";
    [ObservableProperty] private ExpenseType _type        = ExpenseType.Expense;
    [ObservableProperty] private DateTime    _occurredAt  = DateTime.Today;
    [ObservableProperty] private Guid _categoryId = Guid.Empty;
    //[ObservableProperty] private List<CategoryDto> Categories = new();
    public ObservableCollection<CategoryDto> Categories { get; } = new ObservableCollection<CategoryDto>();
    [ObservableProperty] private CategoryDto? selectedCategory;
    public ExpenseDetailViewModel(IExpenseService expenses)
    {
        _expenses = expenses;
    }
    // Cargar categorías al inicializar
    public async Task InitializeAsync(CancellationToken ct)
    {
        try
        {
            Categories.Clear();
            var result1 = await _expenses.GetCategoriesAsync(ct);

            foreach (var e in result1.Items)
                Categories.Add(e);
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudieron cargar las categorías.";
        }
    }

    partial void OnExpenseIdChanged(string? value)
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
        //Categories = result.Items.ToList();
        if (string.IsNullOrEmpty(ExpenseId)) return;
        IsBusy = true;
        try
        {
            // La API no expone GET /expenses/{id} directo; obtenemos la lista paginada con la descripcion
            // Para la fase 6 cargamos desde la lista reciente y filtramos por ID.
            // En Fase 7 se agregara endpoint individual.
            var result = await _expenses.GetPersonalAsync(
                new ExpenseFilter { Page = 1, PageSize = 100 }, ct);

            var expense = result.Items.FirstOrDefault(e => e.Id.ToString() == ExpenseId);
            if (expense is null) return;

            CategoryId = expense.CategoryId;            
            Description = expense.Description;
            Amount      = expense.Amount;
            Currency    = expense.Currency;
            Type        = expense.Type;
            OccurredAt  = expense.OccurredAt.LocalDateTime;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave => !string.IsNullOrWhiteSpace(Description) && Amount > 0 && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync(CancellationToken ct)
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {           
            if (IsEditMode && Guid.TryParse(ExpenseId, out var id))
            {
                await _expenses.UpdateAsync(id, new UpdateExpenseRequest(
                    SelectedCategory.Id, Type, Amount, Currency, Description,
                    new DateTimeOffset(OccurredAt)), ct);
            }
            else
            {
                await _expenses.CreateAsync(new CreateExpenseRequest(
                    SelectedCategory.Id, Type, Amount, Currency, Description,
                    new DateTimeOffset(OccurredAt)), ct);
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception e)
        {
            ErrorMessage = "No se pudo guardar el gasto " + e.Message;
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
