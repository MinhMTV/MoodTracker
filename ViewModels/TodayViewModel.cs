using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoodTracker.Models;
using MoodTracker.Services;

namespace MoodTracker.ViewModels;

public partial class TodayViewModel : ObservableObject
{
    private readonly ISqliteDatabaseService _database;
    private readonly IAiService _aiService;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Now;

    [ObservableProperty]
    private Entry? _todayEntry;

    [ObservableProperty]
    private bool _hasEntry;

    [ObservableProperty] 
    private string _dateLabel = "Heute";

    public TodayViewModel(ISqliteDatabaseService database, IAiService aiService)
    {
        _database = database;
        _aiService = aiService;
    }

    public async Task LoadTodayEntryAsync()
    {
        await _database.InitializeAsync();
        TodayEntry = await _database.GetEntryForDateAsync(SelectedDate);
        HasEntry = TodayEntry != null;
        UpdateDateLabel();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        UpdateDateLabel();
        _ = LoadTodayEntryAsync();
    }

    private void UpdateDateLabel()
    {
        var today = DateTime.Now.Date;
        if (SelectedDate.Date == today)
            DateLabel = "Heute";
        else if (SelectedDate.Date == today.AddDays(-1))
            DateLabel = "Gestern";
        else if (SelectedDate.Date == today.AddDays(1))
            DateLabel = "Morgen";
        else
            DateLabel = SelectedDate.ToString("dddd, dd. MMMM", new System.Globalization.CultureInfo("de-DE"));
    }

    [RelayCommand]
    private async Task NavigateToNewEntryAsync()
    {
        var navigationParameter = new Dictionary<string, object> { { "date", SelectedDate } };
        if (TodayEntry != null)
            navigationParameter.Add("entryId", TodayEntry.Id);
        
        await Shell.Current.GoToAsync("newentry", navigationParameter);
    }

    [RelayCommand]
    private async Task NavigateToPreviousDayAsync()
    {
        SelectedDate = SelectedDate.AddDays(-1);
    }

    [RelayCommand]
    private async Task NavigateToNextDayAsync()
    {
        if (SelectedDate.Date < DateTime.Now.Date)
            SelectedDate = SelectedDate.AddDays(1);
    }

    [RelayCommand]
    private async Task NavigateToTodayAsync()
    {
        SelectedDate = DateTime.Now;
    }

    [RelayCommand]
    private async Task GenerateAiSummaryAsync()
    {
        if (TodayEntry == null || string.IsNullOrWhiteSpace(TodayEntry.Note)) return;
        
        TodayEntry.AiSummary = await _aiService.SummarizeEntryAsync(TodayEntry);
        await _database.SaveEntryAsync(TodayEntry);
        OnPropertyChanged(nameof(TodayEntry));
    }
}
