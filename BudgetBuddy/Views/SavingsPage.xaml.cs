using BudgetBuddy.ViewModels;
using Microsoft.Maui.Controls;

namespace BudgetBuddy.Views;

public partial class SavingsPage : ContentPage
{
    private readonly SavingsViewModel _vm;

    public SavingsPage(SavingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadData();
    }
}