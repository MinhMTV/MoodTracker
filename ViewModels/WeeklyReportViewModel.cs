using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoodTracker.Models;
using MoodTracker.Services;
using System.Collections.ObjectModel;

namespace MoodTracker.ViewModels;

public partial class WeeklyReportViewModel : ObservableObject
{
    private readonly ISqliteDatabaseService _database;
    private readonly IAiService _aiService;

    [ObservableProperty]
    private WeeklyReport? _currentReport;

    [ObservableProperty]
    private ObservableCollection<WeeklyReport> _pastReports = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasReport;

    [ObservableProperty]
    private DateTime _weekStart;

    public WeeklyReportViewModel(ISqliteDatabaseService database, IAiService aiService)
    {
        _database = database;
        _aiService = aiService;
        WeekStart = GetWeekStart(DateTime.Now);
    }

    private DateTime GetWeekStart(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    public async Task LoadReportsAsync()
    {
        await _database.InitializeAsync();
        
        // Load current or generate
        CurrentReport = await _database.GetWeeklyReportAsync(WeekStart);
        
        if (CurrentReport == null)
        {
            await GenerateCurrentReportAsync();
        }
        
        HasReport = CurrentReport != null;
        
        // Load past reports
        var reports = await _database.GetWeeklyReportsAsync(10);
        PastReports = new ObservableCollection<WeeklyReport>(reports.Where(r => r.WeekStart != WeekStart));
    }

    private async Task GenerateCurrentReportAsync()
    {
        IsLoading = true;
        
        try
        {
            var entries = await _database.GetEntriesForWeekAsync(WeekStart);
            
            if (entries.Any())
            {
                CurrentReport = await _aiService.GenerateWeeklyReportAsync(entries, WeekStart);
                await _database.SaveWeeklyReportAsync(CurrentReport);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshReportAsync()
    {
        await GenerateCurrentReportAsync();
        HasReport = CurrentReport != null;
    }

    [RelayCommand]
    private async Task NavigateToWeekAsync(string direction)
    {
        if (direction == "prev")
            WeekStart = WeekStart.AddDays(-7);
        else if (direction == "next" && WeekStart < GetWeekStart(DateTime.Now))
            WeekStart = WeekStart.AddDays(7);
        
        await LoadReportsAsync();
    }

    [RelayCommand]
    private async Task MarkAsReadAsync()
    {
        if (CurrentReport != null)
        {
            CurrentReport.IsRead = true;
            await _database.SaveWeeklyReportAsync(CurrentReport);
        }
    }

    [RelayCommand]
    private async Task OpenPastReportAsync(WeeklyReport report)
    {
        WeekStart = report.WeekStart;
        CurrentReport = report;
        HasReport = true;
        PastReports = new ObservableCollection<WeeklyReport>(
            PastReports.Where(r => r.WeekStart != WeekStart));
    }
}
