using GastosApp.Mobile.Services;

namespace GastosApp.Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly IAuthService _auth;

    public App(IAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell();
        return new Window(shell);
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // Si hay un token en SecureStorage intentamos refrescarlo silenciosamente.
        // Si falla, redirigimos a Login.
        var token = await _auth.GetAccessTokenAsync();
        if (token is not null)
        {
            var ok = await _auth.TryRefreshAsync();
            if (!ok)
                await NavigateToLoginAsync();
            else
                await Shell.Current.GoToAsync("//dashboard");
        }
        else
        {
            await NavigateToLoginAsync();
        }
    }

    private static Task NavigateToLoginAsync()
        => Shell.Current.GoToAsync("//login");
}
