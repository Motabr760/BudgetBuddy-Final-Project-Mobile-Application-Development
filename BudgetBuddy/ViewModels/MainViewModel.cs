using System.Collections.ObjectModel;
using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using SkiaSharp;

namespace BudgetBuddy.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DatabaseService _db;

    // Slice colors used for the pie chart
    private static readonly string[] CategoryColors =
    {
        "#3498db", "#e74c3c", "#2ecc71", "#f39c12",
        "#9b59b6", "#1abc9c", "#e67e22", "#34495e"
    };

    [ObservableProperty]
    private ObservableCollection<Transaction> transactions = new();

    [ObservableProperty]
    private double balance;

    [ObservableProperty]
    private double totalIncome;

    [ObservableProperty]
    private double totalExpenses;

    [ObservableProperty]
    private bool hasTransactions;

    [ObservableProperty]
    private bool hasExpenses;

    [ObservableProperty]
    private Chart? spendingChart;

    public MainViewModel(DatabaseService db)
    {
        _db = db;
    }

    public async Task LoadData()
    {
        await _db.Init();

        var allTransactions = await _db.GetTransactionsAsync();
        var ordered = allTransactions.OrderByDescending(t => t.Date).ToList();

        // Update the existing collection in place — replacing the
        // ObservableCollection forces CollectionView to rebuild every cell,
        // which can stall the UI thread on slow emulators.
        Transactions.Clear();
        foreach (var t in ordered)
            Transactions.Add(t);

        TotalIncome = ordered.Where(t => t.IsIncome).Sum(t => t.Amount);
        TotalExpenses = ordered.Where(t => !t.IsIncome).Sum(t => t.Amount);
        Balance = TotalIncome - TotalExpenses;

        HasTransactions = ordered.Count > 0;

        var expenseGroups = ordered
            .Where(t => !t.IsIncome)
            .GroupBy(t => string.IsNullOrWhiteSpace(t.Category) ? "Other" : t.Category)
            .ToList();

        HasExpenses = expenseGroups.Count > 0;

        // Microcharts can hang/throw on empty entries — only build a chart
        // when there's something to show.
        if (!HasExpenses)
        {
            SpendingChart = null;
            return;
        }

        var chartEntries = expenseGroups
            .Select((g, i) => new ChartEntry((float)g.Sum(t => t.Amount))
            {
                Label = g.Key,
                ValueLabel = g.Sum(t => t.Amount).ToString("C0"),
                Color = SKColor.Parse(CategoryColors[i % CategoryColors.Length]),
            })
            .ToList();

        SpendingChart = new DonutChart
        {
            Entries = chartEntries,
            LabelTextSize = 28,
            BackgroundColor = SKColors.Transparent,
        };
    }

    [RelayCommand]
    private Task Refresh() => LoadData();

    [RelayCommand]
    private async Task AddTransaction()
    {
        await Shell.Current.GoToAsync(nameof(AddTransactionPage));
    }

    [RelayCommand]
    private async Task EditTransaction(Transaction? transaction)
    {
        if (transaction is null) return;
        await Shell.Current.GoToAsync(
            $"{nameof(AddTransactionPage)}?{nameof(AddTransactionViewModel.TransactionId)}={transaction.Id}");
    }

    [RelayCommand]
    private async Task DeleteTransaction(Transaction? transaction)
    {
        if (transaction is null) return;
        await _db.DeleteTransactionAsync(transaction);
        await LoadData();
    }
}
