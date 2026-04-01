using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoodTracker.Models;
using MoodTracker.Services;
using System.Collections.ObjectModel;

namespace MoodTracker.ViewModels;

[QueryProperty("EntryId", "entryId")]
[QueryProperty("Date", "date")]
public partial class NewEntryViewModel : ObservableObject, IQueryAttributable
{
    private readonly ISqliteDatabaseService _database;
    private readonly IAiService _aiService;
    private readonly IVoiceRecordingService _voiceService;

    [ObservableProperty]
    private DateTime _entryDate = DateTime.Now;

    [ObservableProperty]
    private int _mood = 3;

    [ObservableProperty]
    private int _energy = 5;

    [ObservableProperty]
    private int _sleep = 7;

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Activity> _availableActivities = new();

    [ObservableProperty]
    private ObservableCollection<Activity> _selectedActivities = new();

    [ObservableProperty]
    private string _aiSummary = string.Empty;

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private string _voicePath = string.Empty;

    [ObservableProperty]
    private bool _hasVoiceRecording;

    [ObservableProperty]
    private bool _isGeneratingAi;

    public string MoodEmoji => Mood switch { 1 => "😢", 2 => "😕", 3 => "😐", 4 => "🙂", 5 => "😊", _ => "😐" };
    public string MoodLabel => Mood switch { 1 => "Sehr schlecht", 2 => "Schlecht", 3 => "Neutral", 4 => "Gut", 5 => "Sehr gut", _ => "Neutral" };
    public string EnergyEmoji => Energy switch { <= 3 => "🔋", <= 6 => "🔋🔋", _ => "🔋🔋🔋" };
    public Color MoodColor => Mood switch { 1 => Colors.Red, 2 => Colors.Orange, 3 => Colors.Yellow, 4 => Colors.LightGreen, 5 => Colors.Green, _ => Colors.Gray };

    private int _entryId = 0;

    public NewEntryViewModel(ISqliteDatabaseService database, IAiService aiService, IVoiceRecordingService voiceService)
    {
        _database = database;
        _aiService = aiService;
        _voiceService = voiceService;
    }

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();
        AvailableActivities = new ObservableCollection<Activity>(await _database.GetActivitiesAsync());
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("date", out var date))
            EntryDate = (DateTime)date;
        
        if (query.TryGetValue("entryId", out var id))
        {
            _entryId = (int)id;
            _ = LoadEntryAsync(_entryId);
        }
    }

    private async Task LoadEntryAsync(int id)
    {
        var entry = await _database.GetEntryByIdAsync(id);
        if (entry != null)
        {
            Mood = entry.Mood;
            Energy = entry.Energy;
            Sleep = entry.Sleep;
            Note = entry.Note;
            EntryDate = entry.CreatedAt;
            SelectedActivities = new ObservableCollection<Activity>(
                AvailableActivities.Where(a => entry.Activities.Contains(a.Name)));
            VoicePath = entry.LocalVoicePath;
            HasVoiceRecording = !string.IsNullOrEmpty(VoicePath);
        }
    }

    [RelayCommand]
    private void SelectMood(int mood)
    {
        Mood = mood;
    }

    [RelayCommand]
    private void ToggleActivity(Activity activity)
    {
        if (SelectedActivities.Contains(activity))
            SelectedActivities.Remove(activity);
        else
            SelectedActivities.Add(activity);
    }

    [RelayCommand]
    private async Task ToggleRecordingAsync()
    {
        if (IsRecording)
        {
            VoicePath = await _voiceService.StopRecordingAsync();
            HasVoiceRecording = !string.IsNullOrEmpty(VoicePath);
            IsRecording = false;
        }
        else
        {
            await _voiceService.RequestPermissionsAsync();
            await _voiceService.StartRecordingAsync();
            IsRecording = true;
        }
    }

    [RelayCommand]
    private void DeleteVoice()
    {
        VoicePath = string.Empty;
        HasVoiceRecording = false;
    }

    [RelayCommand]
    private async Task GenerateAiSummaryAsync()
    {
        if (string.IsNullOrWhiteSpace(Note)) return;
        
        IsGeneratingAi = true;
        var tempEntry = new Entry { Mood = Mood, Energy = Energy, Note = Note, Activities = SelectedActivities.Select(a => a.Name).ToList() };
        AiSummary = await _aiService.SummarizeEntryAsync(tempEntry);
        IsGeneratingAi = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var entry = new Entry
        {
            Id = _entryId,
            Mood = Mood,
            Energy = Energy,
            Sleep = Sleep,
            Note = Note,
            CreatedAt = EntryDate,
            LocalVoicePath = VoicePath,
            Activities = SelectedActivities.Select(a => a.Name).ToList(),
            AiSummary = AiSummary
        };

        await _database.SaveEntryAsync(entry);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
