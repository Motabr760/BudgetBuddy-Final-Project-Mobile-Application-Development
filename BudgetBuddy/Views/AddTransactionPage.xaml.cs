using BudgetBuddy.ViewModels;

namespace BudgetBuddy.Views;

public partial class AddTransactionPage : ContentPage
{
    private readonly AddTransactionViewModel _vm;

    public AddTransactionPage(AddTransactionViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
