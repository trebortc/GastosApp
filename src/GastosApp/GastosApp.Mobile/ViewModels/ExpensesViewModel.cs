using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GastosApp.Application.Expenses;
using GastosApp.Mobile.Services;
using System.Collections.ObjectModel;

namespace GastosApp.Mobile.ViewModels;

public sealed partial class ExpensesViewModel : ObservableObject
{
    private readonly IExpenseService _expenses;

    [ObservableProperty] private bool    _isBusy;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _searchText;
    [ObservableProperty] private int     _currentPage = 1;
    [ObservableProperty] private int     _totalCount;
    [ObservableProperty] private bool    _hasMore;

    public ObservableCollection<ExpenseDto> Expenses { get; } = [];

    public ExpensesViewModel(IExpenseService expenses)
    {
        _expenses = expenses;
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsBusy = true;
        ErrorMessage = null;
        CurrentPage  = 1;
        Expenses.Clear();
        try
        {
            var result = await _expenses.GetPersonalAsync(
                new ExpenseFilter { Search = SearchText, Page = 1, PageSize = 25 }, ct);

            foreach (var e in result.Items)
                Expenses.Add(e);

            TotalCount = result.TotalCount;
            HasMore    = result.Items.Count < result.TotalCount;
        }
        catch (Exception)
        {
            ErrorMessage = "No se pudieron cargar los gastos.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreAsync(CancellationToken ct)
    {
        if (!HasMore || IsBusy) return;
        IsBusy = true;
        try
        {
            CurrentPage++;
            var result = await _expenses.GetPersonalAsync(
                new ExpenseFilter { Search = SearchText, Page = CurrentPage, PageSize = 25 }, ct);

            foreach (var e in result.Items)
                Expenses.Add(e);

            HasMore = Expenses.Count < TotalCount;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task AddExpenseAsync()
        => await Shell.Current.GoToAsync("expensedetail");

    [RelayCommand]
    private static async Task OpenExpenseAsync(ExpenseDto expense)
        => await Shell.Current.GoToAsync($"expensedetail?id={expense.Id}");

    [RelayCommand]
    private async Task DeleteExpenseAsync(ExpenseDto expense, CancellationToken ct)
    {
        bool confirmed = await Shell.Current.DisplayAlert(
            "Eliminar gasto",
            $"Eliminar \"{expense.Description}\"?",
            "Eliminar", "Cancelar");

        if (!confirmed) return;

        try
        {
            await _expenses.DeleteAsync(expense.Id, ct);
            Expenses.Remove(expense);
            TotalCount--;
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo eliminar el gasto.", "OK");
        }
    }
}
