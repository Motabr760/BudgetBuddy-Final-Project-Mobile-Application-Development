using System.Globalization;
using BudgetBuddy.Data;
using BudgetBuddy.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetBuddy.ViewModels;

public partial class BudgetViewModel : BaseViewModel
{
    private readonly DatabaseService _db;

    [ObservableProperty] private string monthlyLimit = string.Empty;
    [ObservableProperty] private double totalSpent;
    [ObservableProperty] private double remaining;
    [ObservableProperty] private double progress;
    [ObservableProperty] private string currentMonthLabel = string.Empty;
    [ObservableProperty] private bool isOverBudget;

    public BudgetViewModel(DatabaseService db)
    {
        _db = db;
    }

    public async Task LoadData()
    {
        await _db.Init();

        CurrentMonthLabel = DateTime.Now.ToString("MMMM yyyy");
        var currentMonth = DateTime.Now.ToString("yyyy-MM");

        var budgets = await _db.GetBudgetsAsync();
        var budget = budgets.FirstOrDefault(b => b.Month == currentMonth);
        if (budget is not null)
            MonthlyLimit = budget.Limit.ToString("F2", CultureInfo.InvariantCulture);

        var transactions = await _db.GetTransactionsAsync();
        TotalSpent = transactions
            .Where(t => !t.IsIncome && t.Date.ToString("yyyy-MM") == currentMonth)
            .Sum(t => t.Amount);

        Recalculate();
    }

    [RelayCommand]
    private async Task SaveBudget()
    {
        if (!double.TryParse(MonthlyLimit, NumberStyles.Any, CultureInfo.InvariantCulture, out var limit) || limit < 0)
        {
            await Shell.Current.DisplayAlert("Invalid amount", "Please enter a valid monthly limit.", "OK");
            return;
        }

        await _db.Init();

        var currentMonth = DateTime.Now.ToString("yyyy-MM");
        var budgets = await _db.GetBudgetsAsync();
        var existing = budgets.FirstOrDefault(b => b.Month == currentMonth);
        if (existing is not null)
        {
            existing.Limit = limit;
            await _db.UpdateAsync(existing);
        }
        else
        {
            await _db.InsertAsync(new Budget { Month = currentMonth, Limit = limit });
        }

        await LoadData();
        await Shell.Current.DisplayAlert("Saved", "Your monthly budget has been saved.", "OK");
    }

    private void Recalculate()
    {
        if (double.TryParse(MonthlyLimit, NumberStyles.Any, CultureInfo.InvariantCulture, out var limit) && limit > 0)
        {
            Remaining = limit - TotalSpent;
            Progress = Math.Clamp(TotalSpent / limit, 0, 1);
            IsOverBudget = TotalSpent > limit;
        }
        else
        {
            Remaining = 0;
            Progress = 0;
            IsOverBudget = false;
        }
    }
}
