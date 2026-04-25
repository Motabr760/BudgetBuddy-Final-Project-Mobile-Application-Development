using SQLite;

namespace BudgetBuddy.Models;

public class SavingsGoal
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public double TargetAmount { get; set; }
    public double CurrentAmount { get; set; }

    /// <summary>
    /// Raw progress fraction. Can exceed 1.0 when over-funded.
    /// </summary>
    [Ignore]
    public double PercentComplete =>
        TargetAmount <= 0 ? 0 : Math.Max(0, CurrentAmount / TargetAmount);

    /// <summary>
    /// Progress value for the ProgressBar control (clamped to 0-1).
    /// </summary>
    [Ignore]
    public double Progress => Math.Clamp(PercentComplete, 0, 1);

    [Ignore]
    public bool IsGoalReached => CurrentAmount >= TargetAmount && TargetAmount > 0;

    [Ignore]
    public string ProgressText =>
        IsGoalReached
            ? $"{CurrentAmount:C} of {TargetAmount:C} — Goal reached! ({PercentComplete:P0})"
            : $"{CurrentAmount:C} of {TargetAmount:C} ({PercentComplete:P0})";
}
