using MoodTracker.Models;

namespace MoodTracker.Services;

public interface ICloudBackupService
{
    Task<bool> IsAvailableAsync();
    Task<bool> RequestPermissionAsync();
    
    // Full local backup
    Task<string> CreateLocalBackupAsync();
    Task RestoreFromLocalBackupAsync(string backupPath);
    
    // Cloud backup (platform specific)
    Task<bool> BackupToCloudAsync();
    Task<List<BackupInfo>> ListCloudBackupsAsync();
    Task<bool> RestoreFromCloudAsync(BackupInfo backup);
    Task<bool> DeleteCloudBackupAsync(BackupInfo backup);
    
    // Auto backup
    Task ScheduleAutoBackupAsync();
    Task<bool> ShouldAutoBackupAsync();
    Task<DateTime?> GetLastBackupDateAsync();
}
