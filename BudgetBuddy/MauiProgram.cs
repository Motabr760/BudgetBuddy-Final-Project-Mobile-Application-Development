using BudgetBuddy.Data;
using BudgetBuddy.ViewModels;
using BudgetBuddy.Views;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace BudgetBuddy;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        SQLitePCL.Batteries_V2.Init();

        // Services
        builder.Services.AddSingleton<DatabaseService>();

        // ViewModels — transient so each navigation gets a fresh instance and
        // doesn't pin large object graphs (charts, collections) in memory.
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<AddTransactionViewModel>();
        builder.Services.AddTransient<SavingsViewModel>();
        builder.Services.AddTransient<BudgetViewModel>();

        // Pages — also transient. Singleton pages used inside a Shell TabBar
        // can confuse the visual tree and trigger heavy relayouts on tab switches.
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<AddTransactionPage>();
        builder.Services.AddTransient<SavingsPage>();
        builder.Services.AddTransient<BudgetPage>();

        // Shell
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
