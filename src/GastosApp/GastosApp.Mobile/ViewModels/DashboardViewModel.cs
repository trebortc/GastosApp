using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GastosApp.Application.Reports;
using GastosApp.Mobile.Services;

namespace GastosApp.Mobile.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    private readonly IReportService  _reports;
    private readonly IAuthService    _auth;

    [ObservableProperty] private decimal _totalIncome;
    [ObservableProperty] private decimal _totalExpenses;
    [ObservableProperty] private decimal _balance;
    [ObservableProperty] private string  _currency = "MXN";
    [ObservableProperty] private bool    _isBusy;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private IReadOnlyList<CategorySummaryDto> _categoryBreakdown = [];
    [ObservableProperty] private IReadOnlyList<MonthlySummaryDto>  _monthlySummaries  = [];

    // Periodo: mes actual por defecto
    private DateTimeOffset _from = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
    private DateTimeOffset _to   = DateTimeOffset.UtcNow;

    public DashboardViewModel(IReportService reports, IAuthService auth)
    {
        _reports = reports;
        _auth    = auth;
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var data = await _reports.GetDashboardAsync(_from, _to, Currency, ct);
            TotalIncome    = data.TotalIncome;
            TotalExpenses  = data.TotalExpenses;
            Balance        = data.NetBalance;
            CategoryBreakdown = data.ByCategory;
            MonthlySummaries  = data.Monthly;
        }
        catch (Exception)
        {
            ErrorMessage = "No se pudo cargar el resumen. Verifica tu conexion.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task GoToExpensesAsync()
        => await Shell.Current.GoToAsync("//expenses");

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
