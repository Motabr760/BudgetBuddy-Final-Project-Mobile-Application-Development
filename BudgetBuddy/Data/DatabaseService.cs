using BudgetBuddy.Models;
using SQLite;

namespace BudgetBuddy.Data;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;

    public async Task Init()
    {
        if (_database != null) return;

        SQLitePCL.Batteries_V2.Init();
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "BudgetBuddy.db3");
        _database = new SQLiteAsyncConnection(databasePath);

        await _database.CreateTableAsync<Transaction>();

        // The Category model previously had no primary key. If the existing
        // database on disk was created under that old schema, CreateTableAsync
        // will try to ALTER TABLE to add an Id PK column, which SQLite refuses
        // ("Cannot add a PRIMARY KEY column"). Detect that specific failure,
        // drop the legacy table, and recreate it from scratch. Category is
        // seed-only data so this is safe.
        try
        {
            await _database.CreateTableAsync<Category>();
        }
        catch (SQLiteException ex) when (ex.Message.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase))
        {
            await _database.DropTableAsync<Category>();
            await _database.CreateTableAsync<Category>();
        }

        await _database.CreateTableAsync<SavingsGoal>();
        await _database.CreateTableAsync<Budget>();

        await SeedDefaultsAsync();
    }

    private async Task SeedDefaultsAsync()
    {
        if (_database == null) return;

        var existingCategories = await _database.Table<Category>().ToListAsync();
        if (!existingCategories.Any())
        {
            var defaultCategories = new List<Category>
            {
                new() { Name = "Food" },
                new() { Name = "Transport" },
                new() { Name = "Entertainment" },
                new() { Name = "Bills" },
                new() { Name = "Shopping" },
                new() { Name = "Health" },
                new() { Name = "Income" },
                new() { Name = "Other" },
            };
            foreach (var cat in defaultCategories)
                await _database.InsertAsync(cat);
        }
    }

    private SQLiteAsyncConnection Db =>
        _database ?? throw new InvalidOperationException("DatabaseService not initialized. Call Init() first.");

    // Generic helpers
    public Task<List<T>> GetAllAsync<T>() where T : new() => Db.Table<T>().ToListAsync();
    public Task<int> InsertAsync<T>(T item) => Db.InsertAsync(item);
    public Task<int> UpdateAsync<T>(T item) => Db.UpdateAsync(item);
    public Task<int> DeleteAsync<T>(T item) => Db.DeleteAsync(item);

    // Transactions
    public Task<List<Transaction>> GetTransactionsAsync() => Db.Table<Transaction>().ToListAsync();
    public Task<int> AddTransactionAsync(Transaction transaction) => Db.InsertAsync(transaction);
    public Task<int> UpdateTransactionAsync(Transaction transaction) => Db.UpdateAsync(transaction);
    public Task<int> DeleteTransactionAsync(Transaction transaction) => Db.DeleteAsync(transaction);

    // Categories
    public Task<List<Category>> GetCategoriesAsync() => Db.Table<Category>().ToListAsync();

    // Budgets
    public Task<List<Budget>> GetBudgetsAsync() => Db.Table<Budget>().ToListAsync();

    // Savings goals
    public Task<List<SavingsGoal>> GetSavingsGoalsAsync() => Db.Table<SavingsGoal>().ToListAsync();
}
