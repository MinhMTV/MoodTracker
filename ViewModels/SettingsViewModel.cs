using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoodTracker.Services;

namespace MoodTracker.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ICloudBackupService _backupService;

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private bool _autoBackup;

    [ObservableProperty]
    private bool _isBackingUp;

    public SettingsViewModel(ICloudBackupService backupService)
    {
        _backupService = backupService;
        ApiKey = UserSecrets.OpenRouterApiKey ?? string.Empty;
        AutoBackup = Preferences.Get("auto_backup", true);
    }

    partial void OnApiKeyChanged(string value) => UserSecrets.SetOpenRouterApiKey(value);
    partial void OnAutoBackupChanged(bool value) => Preferences.Set("auto_backup", value);

    [RelayCommand]
    private async Task BackupNowAsync()
    {
        IsBackingUp = true;
        var success = await _backupService.BackupToCloudAsync();
        IsBackingUp = false;
        await Shell.Current.DisplayAlert(success ? "Erfolg" : "Fehler", 
            success ? "Backup erstellt!" : "Backup fehlgeschlagen.", "OK");
    }

    [RelayCommand]
    private async Task ExportToFileAsync()
    {
        var path = await _backupService.CreateLocalBackupAsync();
        await Shell.Current.DisplayAlert("Export", $"Gespeichert unter: {path}", "OK");
    }
}
