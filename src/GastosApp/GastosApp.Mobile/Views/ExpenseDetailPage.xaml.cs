using AndroidX.Lifecycle;
using GastosApp.Mobile.ViewModels;
using static Android.App.Assist.AssistStructure;

namespace GastosApp.Mobile.Views;

public partial class ExpenseDetailPage : ContentPage
{
    private readonly ExpenseDetailViewModel _viewModel;
    public ExpenseDetailPage(ExpenseDetailViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = vm;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_viewModel.IsBusy && _viewModel.Categories.Count == 0)
            await _viewModel.InitializeAsync(CancellationToken.None);
    }
}
