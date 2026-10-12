using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GastosApp.Mobile.Services;

namespace GastosApp.Mobile.ViewModels;

public sealed partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthService _auth;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _currency = "MXN";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    public RegisterViewModel(IAuthService auth)
    {
        _auth = auth;
    }

    private bool CanRegister => !string.IsNullOrWhiteSpace(Email) &&
                                Password.Length >= 8 &&
                                !string.IsNullOrWhiteSpace(DisplayName) &&
                                !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task RegisterAsync(CancellationToken ct)
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _auth.RegisterAsync(Email.Trim(), Password, DisplayName.Trim(), Currency, ct);
            await Shell.Current.GoToAsync("//dashboard");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            ErrorMessage = "Ya existe una cuenta con ese correo.";
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Error de conexion. Verifica tu red.";
        }
        catch (Exception)
        {
            ErrorMessage = "Ocurrio un error inesperado.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task GoBackAsync()
        => await Shell.Current.GoToAsync("..");
}
