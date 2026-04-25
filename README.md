# BudgetBuddy
> A small, private, offline personal-finance app built with **.NET MAUI 8**.
> Track money in and money out, set a monthly spending limit, and watch
> progress toward savings goals — all stored locally with SQLite, no account
> required, no internet needed.

![Platform](https://img.shields.io/badge/platform-Android%20%7C%20Windows-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

## Project Goals

BudgetBuddy is a focused, opinionated budgeting app for the user who just
wants to write down what they spent, see a chart of where the money went,
and stay under a self-imposed monthly limit. The goals of the project are:

- **At-a-glance financial picture.** A single dashboard that answers
  "how am I doing this month?" in under three seconds — current balance,
  total income, total expenses, and a category breakdown chart.
- **Friction-free transaction entry.** Adding, editing, or deleting a
  transaction takes fewer than five taps.
- **Visible monthly limits.** Set a monthly spending cap and immediately
  see, in colour, whether you're inside it or over it.
- **Lightweight savings tracking.** Create short-term goals (an emergency
  fund, a new bike), contribute to them over time, and get a clear visual
  cue when a goal is reached.
- **Privacy by default.** All data lives in a single SQLite file on your
  device. No account, no sign-in, no internet permission, no telemetry.

## Screens

| Dashboard | Budget | Savings |
|-----------|--------|---------|
| ![Dashboard](docs/wireframe_dashboard.png) | ![Budget](docs/wireframe_budget.png) | ![Savings](docs/wireframe_savings.png) |

### Navigation

![Navigation diagram](docs/nav_diagram.png)

The app uses MAUI Shell with three top-level tabs (Dashboard, Budget,
Savings). The Add / Edit Transaction page is a child route reached with
`Shell.Current.GoToAsync("AddTransactionPage")` and an optional
`?TransactionId={id}` query parameter for edit mode.

## Features

### Dashboard
- Balance card showing current balance, total income, and total expenses
- Donut chart of spending by category (Microcharts)
- Recent-transactions list with inline Edit / Delete buttons
- Floating "+" button to add a new transaction
- Refresh button to re-pull data without leaving the page

### Transactions
- Title, amount, category, date, and Income/Expense toggle
- Edit reopens the same Add page with values pre-filled
- One-tap delete from the dashboard list
- Persisted to SQLite immediately

### Monthly Budget
- Per-month spending limit (keyed by `yyyy-MM`, so months don't overwrite each other)
- Live progress bar that fills as expenses accumulate
- **Within budget:** primary-colour bar, green "Remaining" amount
- **Over budget:** crimson bar, crimson negative "Remaining", explicit warning line

### Savings Goals
- Create goals with a name, target amount, and optional starting balance
- Contribute via a numeric prompt — contributions can exceed the target
- Edit a goal's title and target without losing saved progress
- Card shows progress bar plus text like `$650 of $1,000 (65%)`
- When a goal is reached, the bar turns green and the text becomes
  `$1,250 of $1,000 — Goal reached! (125%)`, with a celebration alert

### Cross-cutting
- Light and dark themes via `AppThemeBinding` throughout the XAML
- Local SQLite database, created and migrated automatically on first run
- Self-healing migration for the legacy `Category` table
- Fully offline — no API calls, no login, no telemetry

## Tech Stack

- **.NET 8** with **.NET MAUI 8** (Android + Windows targets)
- **MVVM** via `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`)
- **SQLite** via `sqlite-net-pcl` and `SQLitePCLRaw.bundle_green`
- **Microcharts.Maui** + **SkiaSharp** for the donut chart
- **Shell** navigation with a `TabBar`

### NuGet packages

| Package | Why |
|---------|-----|
| `Microsoft.Maui.Controls` | Core MAUI controls — pages, layouts, Shell, CollectionView, etc. |
| `CommunityToolkit.Mvvm` | Source-generated MVVM helpers so view-models stay declarative |
| `sqlite-net-pcl` | Async ORM over SQLite for all persistence |
| `SQLitePCLRaw.bundle_green` | Native SQLite engine that sqlite-net-pcl talks to |
| `Microcharts.Maui` | Chart rendering (DonutChart) for the spending breakdown |
| `SkiaSharp.Views.Maui.Controls` | 2D graphics surface that backs Microcharts |

## Building & Running

### Prerequisites

- **Windows 10 or 11** (for the Windows target; macOS works for Android only)
- **Visual Studio 2022 17.8+** with the **.NET Multi-platform App UI development** workload
  installed. The workload installer pulls in the .NET 8 SDK, the MAUI workloads,
  and the Android SDK.
- **(Android only)** An Android emulator created in **Android Device Manager**, or
  a physical Android phone with USB debugging enabled.
- **(Windows only)** Developer Mode turned on
  (Settings → Privacy & security → For developers → Developer Mode).

### Clone

```bash
git clone https://github.com/<your-username>/BudgetBuddy.git
cd BudgetBuddy
```

### Restore dependencies

```bash
dotnet restore
```

### Run on Windows

The fastest path. From Visual Studio:

1. Open `BudgetBuddy.sln`.
2. In the debug-target dropdown at the top, select **Windows Machine**.
3. Press **F5** (or click the green ▶ button).

From the command line:

```bash
dotnet build BudgetBuddy/BudgetBuddy.csproj -t:Run -f net8.0-windows10.0.19041.0
```

### Run on Android (emulator)

1. Open `BudgetBuddy.sln` in Visual Studio.
2. In the debug-target dropdown, choose your **Android Emulator** (e.g.
   `Pixel 5 - API 34`). If you don't have one, click
   **Android Device Manager → +** to create one.
3. Press **F5**.

> **Tip — emulator performance.** In Android Device Manager, edit your
> AVD and confirm **Graphics: Hardware - GLES 2.0 (or 3.0)**, **2+ CPU
> cores**, and **2 GB+ RAM**. Software graphics will make SkiaSharp/Microcharts
> very slow and can trigger Android's "System UI not responding" warning.

### Run on a Physical Android Device

1. Enable **Developer options** on the phone, then turn on **USB debugging**.
2. Plug the phone into the PC via USB and accept the trust prompt.
3. In Visual Studio's debug-target dropdown, your phone will appear under
   **Android Local Devices**. Select it and press **F5**.

### Build a Release APK (sideload-ready)

```bash
dotnet publish BudgetBuddy/BudgetBuddy.csproj \
  -c Release \
  -f net8.0-android \
  -p:AndroidPackageFormats=apk
```

The signed APK lands in
`BudgetBuddy/bin/Release/net8.0-android/publish/`.

---

## Project Structure

```
BudgetBuddy/
├── BudgetBuddy.sln
├── README.md
├── docs/                       # diagrams & wireframes for this README
└── BudgetBuddy/
    ├── App.xaml                # application root + theme resources
    ├── AppShell.xaml           # TabBar with Dashboard / Budget / Savings
    ├── MainPage.xaml           # Dashboard
    ├── MauiProgram.cs          # DI registration + UseSkiaSharp + UseMauiApp
    ├── Models/                 # Transaction, Category, Budget, SavingsGoal
    ├── Data/
    │   └── DatabaseService.cs  # SQLite init, migrations, typed queries
    ├── ViewModels/             # MainViewModel, BudgetViewModel, SavingsViewModel,
    │                           # AddTransactionViewModel, BaseViewModel
    ├── Views/                  # BudgetPage, SavingsPage, AddTransactionPage
    ├── Helpers/                # InvertBoolConverter, etc.
    ├── Resources/              # Fonts, Colors.xaml, Styles.xaml, images
    └── Platforms/              # Android, Windows entry points
```

## Data Storage

All data lives in a single SQLite file at:

- **Windows:** `%LOCALAPPDATA%\Packages\<package-id>\LocalState\BudgetBuddy.db3`
- **Android:** the app's private storage area (not visible without root)

The schema is created automatically on first launch via
`SQLiteAsyncConnection.CreateTableAsync<T>()`. There is also a self-healing
migration in `DatabaseService.Init()` that drops and recreates the legacy
`Category` table if it was created without a primary key (this only matters
for users who installed pre-1.0 builds).

## Roadmap

Future iterations could add:

- CSV / JSON export of all transactions
- Multi-currency support
- Recurring transactions (rent, salary, subscriptions)
- Push reminders for budget thresholds
- Optional cloud sync via Azure or Supabase

The MVVM layout already keeps the UI and data concerns cleanly separated, so
any of those additions slot in without touching the existing pages.

## Author

**Ben Mota** — built as a class project, April 2026.
