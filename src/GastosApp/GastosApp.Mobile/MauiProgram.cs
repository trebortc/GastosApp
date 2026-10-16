using GastosApp.Mobile.Services;
using GastosApp.Mobile.ViewModels;
using GastosApp.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace GastosApp.Mobile;

public static class MauiProgram
{
    // En desarrollo la API corre en localhost. Cambia esto a tu URL de produccion.
#if DEBUG
    //public const string ApiBaseUrl = "https://10.0.2.2:7001/"; // Android emulator → localhost
    public const string ApiBaseUrl = "https://gastosapp-sx4i.onrender.com";
#else
    public const string ApiBaseUrl = "https://gastosapp-sx4i.onrender.com";
#endif

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // -- App con DI --
        builder.Services.AddSingleton<App>();

        // -- Servicios --
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddTransient<Lazy<IAuthService>>(sp =>
            new Lazy<IAuthService>(() => sp.GetRequiredService<IAuthService>()));
        builder.Services.AddTransient<AuthTokenHandler>();

        builder.Services.AddHttpClient("GastosApi", c =>
        {
            c.BaseAddress = new Uri(ApiBaseUrl);
            c.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<AuthTokenHandler>();

        builder.Services.AddTransient<IExpenseService, ExpenseService>();
        builder.Services.AddTransient<IReportService, ReportService>();

        // -- ViewModels --
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<ExpensesViewModel>();
        builder.Services.AddTransient<ExpenseDetailViewModel>();

        // -- Pages --
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<ExpensesPage>();
        builder.Services.AddTransient<ExpenseDetailPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
