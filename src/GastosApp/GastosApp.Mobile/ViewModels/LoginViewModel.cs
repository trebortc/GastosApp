using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GastosApp.Mobile.Services;

namespace GastosApp.Mobile.ViewModels;

public sealed partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    public LoginViewModel(IAuthService auth)
    {
        _auth = auth;
    }

    private bool CanLogin => !string.IsNullOrWhiteSpace(Email) &&
                             !string.IsNullOrWhiteSpace(Password) &&
                             !IsBusy;

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync(CancellationToken ct)
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _auth.LoginAsync(Email.Trim(), Password, ct);
            await Shell.Current.GoToAsync("//dashboard");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.StatusCode == System.Net.HttpStatusCode.Unauthorized
                ? "Correo o contrasena incorrectos."
                : "Error de conexion. Verifica tu red.";
        }
        catch (Exception e)
        {
            ErrorMessage = "Ocurrio un error inesperado. " + e.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task GoToRegisterAsync()
        => await Shell.Current.GoToAsync("register");
}
