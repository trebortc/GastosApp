using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GastosApp.Application.Expenses;
using GastosApp.Domain.Enums;
using GastosApp.Mobile.Services;

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

    public ExpenseDetailViewModel(IExpenseService expenses)
    {
        _expenses = expenses;
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
            // CategoryId: se usara un picker en pantalla completa; aqui usamos Guid.Empty como placeholder
            // hasta que se implemente la seleccion de categoria.
            var categoryId = Guid.Empty;

            if (IsEditMode && Guid.TryParse(ExpenseId, out var id))
            {
                await _expenses.UpdateAsync(id, new UpdateExpenseRequest(
                    categoryId, Type, Amount, Currency, Description,
                    new DateTimeOffset(OccurredAt)), ct);
            }
            else
            {
                await _expenses.CreateAsync(new CreateExpenseRequest(
                    categoryId, Type, Amount, Currency, Description,
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
