using System.Collections.ObjectModel;
using System.Globalization;
using BudgetBuddy.Data;
using BudgetBuddy.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetBuddy.ViewModels;

public partial class SavingsViewModel : ObservableObject
{
    private readonly DatabaseService _db;

    [ObservableProperty] private ObservableCollection<SavingsGoal> goals = new();
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string targetAmount = string.Empty;
    [ObservableProperty] private string currentAmount = string.Empty;
    [ObservableProperty] private bool hasGoals;

    public SavingsViewModel(DatabaseService db) => _db = db;

    public async Task LoadData()
    {
        await _db.Init();
        var allGoals = await _db.GetSavingsGoalsAsync();

        // Update in place to avoid CollectionView recycling every cell
        Goals.Clear();
        foreach (var g in allGoals)
            Goals.Add(g);

        HasGoals = Goals.Count > 0;
    }

    [RelayCommand]
    private async Task AddGoal()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Shell.Current.DisplayAlert("Missing info", "Please enter a goal title.", "OK");
            return;
        }
        if (!double.TryParse(TargetAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var target) || target <= 0)
        {
            await Shell.Current.DisplayAlert("Invalid amount", "Target amount must be a positive number.", "OK");
            return;
        }

        double.TryParse(CurrentAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var current);

        var goal = new SavingsGoal
        {
            Title = Title.Trim(),
            TargetAmount = target,
            CurrentAmount = Math.Max(0, current),
        };

        await _db.InsertAsync(goal);

        Title = string.Empty;
        TargetAmount = string.Empty;
        CurrentAmount = string.Empty;

        await LoadData();
    }

    [RelayCommand]
    private async Task EditGoal(SavingsGoal? goal)
    {
        if (goal is null) return;

        var newTitle = await Shell.Current.DisplayPromptAsync(
            "Edit goal",
            "Goal title",
            accept: "Next",
            cancel: "Cancel",
            initialValue: goal.Title);

        if (newTitle is null) return; // user cancelled
        newTitle = newTitle.Trim();
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            await Shell.Current.DisplayAlert("Invalid title", "Title can't be blank.", "OK");
            return;
        }

        var newTargetText = await Shell.Current.DisplayPromptAsync(
            "Edit goal",
            "Target amount",
            accept: "Save",
            cancel: "Cancel",
            initialValue: goal.TargetAmount.ToString("0.##", CultureInfo.InvariantCulture),
            keyboard: Keyboard.Numeric);

        if (newTargetText is null) return; // user cancelled
        if (!double.TryParse(newTargetText, NumberStyles.Any, CultureInfo.InvariantCulture, out var newTarget) || newTarget <= 0)
        {
            await Shell.Current.DisplayAlert("Invalid amount", "Target amount must be a positive number.", "OK");
            return;
        }

        goal.Title = newTitle;
        goal.TargetAmount = newTarget;
        await _db.UpdateAsync(goal);
        await LoadData();
    }

    [RelayCommand]
    private async Task DeleteGoal(SavingsGoal? goal)
    {
        if (goal is null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Delete goal",
            $"Delete \"{goal.Title}\"?",
            "Delete", "Cancel");
        if (!confirm) return;

        await _db.DeleteAsync(goal);
        await LoadData();
    }

    [RelayCommand]
    private async Task ContributeToGoal(SavingsGoal? goal)
    {
        if (goal is null) return;

        var result = await Shell.Current.DisplayPromptAsync(
            "Add contribution",
            $"How much would you like to add to \"{goal.Title}\"?",
            accept: "Add",
            cancel: "Cancel",
            placeholder: "0.00",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(result)) return;
        if (!double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) || amount <= 0) return;

        goal.CurrentAmount += amount;
        await _db.UpdateAsync(goal);
        await LoadData();

        if (goal.IsGoalReached)
        {
            await Shell.Current.DisplayAlert(
                "Goal reached!",
                $"Nice work — you've saved {goal.CurrentAmount:C} toward your {goal.TargetAmount:C} goal for \"{goal.Title}\".",
                "OK");
        }
    }
}
