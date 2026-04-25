using SQLite;

namespace BudgetBuddy.Models;

public class Budget
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Month { get; set; } = string.Empty;
    public double Limit { get; set; }
}
