using System.Collections.ObjectModel;
using BudgetBuddy.Data;
using BudgetBuddy.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetBuddy.ViewModels;

[QueryProperty(nameof(TransactionId), nameof(TransactionId))]
public partial class AddTransactionViewModel : ObservableObject
{
    private readonly DatabaseService _db;

    [ObservableProperty] private int transactionId;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private double amount;
    [ObservableProperty] private DateTime date = DateTime.Today;
    [ObservableProperty] private string? selectedCategory;
    [ObservableProperty] private bool isIncome;
    [ObservableProperty] private string pageTitle = "Add Transaction";

    [ObservableProperty]
    private ObservableCollection<string> categories = new();

    public AddTransactionViewModel(DatabaseService db)
    {
        _db = db;
    }

    public async Task LoadAsync()
    {
        await _db.Init();

        var cats = await _db.GetCategoriesAsync();
        Categories = new ObservableCollection<string>(cats.Select(c => c.Name));

        if (TransactionId > 0)
        {
            var all = await _db.GetTransactionsAsync();
            var existing = all.FirstOrDefault(t => t.Id == TransactionId);
            if (existing is not null)
            {
                Title = existing.Title;
                Amount = existing.Amount;
                Date = existing.Date;
                SelectedCategory = existing.Category;
                IsIncome = existing.IsIncome;
                PageTitle = "Edit Transaction";
            }
        }
        else
        {
            PageTitle = "Add Transaction";
        }
    }

    [RelayCommand]
    private async Task SaveTransaction()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Shell.Current.DisplayAlert("Missing info", "Please enter a title.", "OK");
            return;
        }
        if (Amount <= 0)
        {
            await Shell.Current.DisplayAlert("Missing info", "Amount must be greater than zero.", "OK");
            return;
        }

        var transaction = new Transaction
        {
            Id = TransactionId,
            Title = Title.Trim(),
            Amount = Amount,
            Date = Date,
            Category = SelectedCategory ?? "Other",
            IsIncome = IsIncome,
        };

        if (TransactionId != 0)
            await _db.UpdateTransactionAsync(transaction);
        else
            await _db.AddTransactionAsync(transaction);

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}
