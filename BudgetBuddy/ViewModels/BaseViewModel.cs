using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetBuddy.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    bool isBusy;
}
