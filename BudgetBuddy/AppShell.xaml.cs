using BudgetBuddy.Views;

namespace BudgetBuddy;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Routes for navigation that isn't part of the TabBar
        Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
    }
}
