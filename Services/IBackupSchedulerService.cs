namespace MoodTracker.Services;

public interface IBackupSchedulerService
{
    Task StartAutoBackupAsync();
    Task StopAutoBackupAsync();
    Task RunBackupIfNeededAsync();
    bool IsAutoBackupEnabled { get; }
}

public class BackupSchedulerService : IBackupSchedulerService
{
    private readonly ICloudBackupService _cloudBackup;
    private System.Threading.Timer? _backupTimer;

    public BackupSchedulerService(ICloudBackupService cloudBackup)
    {
        _cloudBackup = cloudBackup;
    }

    public bool IsAutoBackupEnabled => Preferences.Get("auto_backup_enabled", true);

    public Task StartAutoBackupAsync()
    {
        _backupTimer?.Dispose();
        _backupTimer = new System.Threading.Timer(
            async _ => await RunBackupIfNeededAsync(),
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromHours(1));
        return Task.CompletedTask;
    }

    public Task StopAutoBackupAsync()
    {
        _backupTimer?.Dispose();
        _backupTimer = null;
        return Task.CompletedTask;
    }

    public async Task RunBackupIfNeededAsync()
    {
        if (!IsAutoBackupEnabled) return;
        if (!await _cloudBackup.ShouldAutoBackupAsync()) return;
        
        await _cloudBackup.BackupToCloudAsync();
        Preferences.Set("last_backup_date", DateTime.UtcNow.ToString("O"));
    }
}
