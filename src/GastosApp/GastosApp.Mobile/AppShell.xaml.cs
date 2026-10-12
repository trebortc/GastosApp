using GastosApp.Mobile.Views;

namespace GastosApp.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rutas modales / de detalle (no en tab bar)
        Routing.RegisterRoute("register",      typeof(RegisterPage));
        Routing.RegisterRoute("expensedetail", typeof(ExpenseDetailPage));
    }
}

