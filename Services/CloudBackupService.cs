using MoodTracker.Models;
using SQLite;
using System.IO.Compression;
using Newtonsoft.Json;

namespace MoodTracker.Services;

public class CloudBackupService : ICloudBackupService
{
    private readonly ISqliteDatabaseService _database;
    private const string BackupFolderName = "MoodTrackerBackups";
    private const string BackupFilePrefix = "mood_backup_";

    public CloudBackupService(ISqliteDatabaseService database)
    {
        _database = database;
    }

    public async Task<bool> IsAvailableAsync()
    {
        // Check if cloud storage is available (iCloud Drive or Google Drive)
        #if IOS
        return true; // iCloud is always available on iOS
        #elif ANDROID
        return await CheckGoogleDriveAvailabilityAsync();
        #else
        return false;
        #endif
    }

    public async Task<bool> RequestPermissionAsync()
    {
        // Platform specific permission handling
        return await Task.FromResult(true);
    }

    public async Task<string> CreateLocalBackupAsync()
    {
        await _database.InitializeAsync();

        var backupFolder = Path.Combine(FileSystem.CacheDirectory, BackupFolderName);
        Directory.CreateDirectory(backupFolder);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupPath = Path.Combine(backupFolder, $"{BackupFilePrefix}{timestamp}.zip");

        // Get all data
        var entries = await _database.GetAllEntriesAsync();
        var activities = await _database.GetActivitiesAsync();
        var reports = await _database.GetWeeklyReportsAsync(1000);

        // Serialize to JSON
        var backupData = new
        {
            Version = "1.0",
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = DeviceInfo.Current.Model,
            Entries = entries,
            Activities = activities,
            WeeklyReports = reports
        };

        var json = JsonConvert.SerializeObject(backupData, Formatting.Indented);
        var tempJsonPath = Path.Combine(FileSystem.CacheDirectory, "backup_data.json");
        await File.WriteAllTextAsync(tempJsonPath, json);

        // Copy voice memos to temp folder
        var voiceFolder = Path.Combine(FileSystem.AppDataDirectory, "voicememos");
        var tempVoiceFolder = Path.Combine(FileSystem.CacheDirectory, "backup_voices");
        if (Directory.Exists(voiceFolder))
        {
            Directory.CreateDirectory(tempVoiceFolder);
            foreach (var voiceFile in Directory.GetFiles(voiceFolder, "*.m4a"))
            {
                var dest = Path.Combine(tempVoiceFolder, Path.GetFileName(voiceFile));
                File.Copy(voiceFile, dest, true);
            }
        }

        // Create ZIP
        using (var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create))
        {
            archive.CreateEntryFromFile(tempJsonPath, "data.json");

            if (Directory.Exists(tempVoiceFolder))
            {
                foreach (var voiceFile in Directory.GetFiles(tempVoiceFolder, "*.m4a"))
                {
                    var entryName = Path.Combine("voices", Path.GetFileName(voiceFile));
                    archive.CreateEntryFromFile(voiceFile, entryName);
                }
            }
        }

        // Cleanup temp files
        File.Delete(tempJsonPath);
        if (Directory.Exists(tempVoiceFolder))
            Directory.Delete(tempVoiceFolder, true);

        return backupPath;
    }

    public async Task RestoreFromLocalBackupAsync(string backupPath)
    {
        if (!File.Exists(backupPath))
            throw new FileNotFoundException("Backup nicht gefunden");

        var tempFolder = Path.Combine(FileSystem.CacheDirectory, "restore");
        if (Directory.Exists(tempFolder))
            Directory.Delete(tempFolder, true);
        Directory.CreateDirectory(tempFolder);

        // Extract ZIP
        ZipFile.ExtractToDirectory(backupPath, tempFolder);

        // Load data
        var jsonPath = Path.Combine(tempFolder, "data.json");
        if (File.Exists(jsonPath))
        {
            var json = await File.ReadAllTextAsync(jsonPath);
            var backup = JsonConvert.DeserializeObject<dynamic>(json);

            // TODO: Restore entries, activities, reports
            // This would merge or replace based on user preference
        }

        // Restore voice memos
        var voicesFolder = Path.Combine(tempFolder, "voices");
        if (Directory.Exists(voicesFolder))
        {
            var destVoiceFolder = Path.Combine(FileSystem.AppDataDirectory, "voicememos");
            Directory.CreateDirectory(destVoiceFolder);

            foreach (var voiceFile in Directory.GetFiles(voicesFolder, "*.m4a"))
            {
                var dest = Path.Combine(destVoiceFolder, Path.GetFileName(voiceFile));
                File.Copy(voiceFile, dest, true);
            }
        }

        // Cleanup
        Directory.Delete(tempFolder, true);
    }

    public async Task<bool> BackupToCloudAsync()
    {
        try
        {
            var localBackup = await CreateLocalBackupAsync();

            #if IOS
            return await BackupToICloudAsync(localBackup);
            #elif ANDROID
            return await BackupToGoogleDriveAsync(localBackup);
            #else
            // Fallback: just keep local backup
            return true;
            #endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Backup failed: {ex}");
            return false;
        }
    }

    public Task<List<BackupInfo>> ListCloudBackupsAsync()
    {
        // Platform specific listing
        return Task.FromResult(new List<BackupInfo>());
    }

    public Task<bool> RestoreFromCloudAsync(BackupInfo backup)
    {
        // Platform specific restore
        return Task.FromResult(false);
    }

    public Task<bool> DeleteCloudBackupAsync(BackupInfo backup)
    {
        // Platform specific delete
        return Task.FromResult(false);
    }

    public async Task ScheduleAutoBackupAsync()
    {
        // Platform specific background task scheduling
        await Task.CompletedTask;
    }

    public async Task<bool> ShouldAutoBackupAsync()
    {
        var lastBackup = await GetLastBackupDateAsync();
        if (lastBackup == null) return true;

        return DateTime.UtcNow - lastBackup.Value > TimeSpan.FromHours(24);
    }

    public Task<DateTime?> GetLastBackupDateAsync()
    {
        // TODO: Store last backup date in preferences
        var lastBackupStr = Preferences.Get("last_backup_date", null);
        if (string.IsNullOrEmpty(lastBackupStr)) return Task.FromResult<DateTime?>(null);

        if (DateTime.TryParse(lastBackupStr, out var date))
            return Task.FromResult<DateTime?>(date);

        return Task.FromResult<DateTime?>(null);
    }

    //