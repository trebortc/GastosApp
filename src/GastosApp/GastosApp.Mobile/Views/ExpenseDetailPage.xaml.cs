using GastosApp.Mobile.ViewModels;

namespace GastosApp.Mobile.Views;

public partial class ExpenseDetailPage : ContentPage
{
    public ExpenseDetailPage(ExpenseDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
