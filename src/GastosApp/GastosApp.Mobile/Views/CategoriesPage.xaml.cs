using GastosApp.Mobile.ViewModels;

namespace GastosApp.Mobile.Views;

public partial class CategoriesPage : ContentPage
{
    private readonly CategoriesViewModel _vm;

    public CategoriesPage(CategoriesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!_vm.IsBusy)
            _vm.LoadCommand.Execute(null);
    }
}
