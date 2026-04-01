using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoodTracker.Models;
using MoodTracker.Services;
using System.Collections.ObjectModel;

namespace MoodTracker.ViewModels;

public partial class TimelineViewModel : ObservableObject
{
    private readonly ISqliteDatabaseService _database;

    [ObservableProperty]
    private ObservableCollection<Entry> _entries = new();

    [ObservableProperty]
    private DateTime _selectedMonth = DateTime.Now;

    [ObservableProperty]
    private string _monthLabel = string.Empty;

    [ObservableProperty]
    private bool _hasEntries;

    public TimelineViewModel(ISqliteDatabaseService database)
    {
        _database = database;
        UpdateMonthLabel();
    }

    public async Task LoadEntriesAsync()
    {
        await _database.InitializeAsync();
        var monthEntries = await _database.GetEntriesForMonthAsync(SelectedMonth.Year, SelectedMonth.Month);
        Entries = new ObservableCollection<Entry>(monthEntries);
        HasEntries = Entries.Any();
    }

    partial void OnSelectedMonthChanged(DateTime value)
    {
        UpdateMonthLabel();
        _ = LoadEntriesAsync();
    }

    private void UpdateMonthLabel()
    {
        MonthLabel = SelectedMonth.ToString("MMMM yyyy", new System.Globalization.CultureInfo("de-DE"));
    }

    [RelayCommand]
    private async Task NavigateToPreviousMonthAsync()
    {
        SelectedMonth = SelectedMonth.AddMonths(-1);
    }

    [RelayCommand]
    private async Task NavigateToNextMonthAsync()
    {
        if (SelectedMonth < DateTime.Now)
            SelectedMonth = SelectedMonth.AddMonths(1);
    }

    [RelayCommand]
    private async Task OpenEntryAsync(Entry entry)
    {
        await Shell.Current.GoToAsync($"newentry?entryId={entry.Id}");
    }

    [RelayCommand]
    private async Task DeleteEntryAsync(Entry entry)
    {
        bool confirm = await Shell.Current.DisplayAlert("Löschen?", "Eintrag wirklich löschen?", "Ja", "Nein");
        if (confirm)
        {
            await _database.DeleteEntryAsync(entry.Id);
            Entries.Remove(entry);
            HasEntries = Entries.Any();
        }
    }
}
