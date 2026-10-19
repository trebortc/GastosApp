using GastosApp.Mobile.ViewModels;

namespace GastosApp.Mobile.Views;

public partial class CategoryDetailPage : ContentPage
{
    public CategoryDetailPage(CategoryDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
