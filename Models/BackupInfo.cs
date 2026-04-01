namespace MoodTracker.Models;

public class BackupInfo
{
    public DateTime BackupDate { get; set; }
    public long BackupSize { get; set; }
    public string BackupPath { get; set; } = string.Empty;
    public int EntryCount { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    
    public string FormattedSize => FormatBytes(BackupSize);
    
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

public class BackupSettings
{
    public bool AutoBackupEnabled { get; set; } = true;
    public int AutoBackupIntervalHours { get; set; } = 24;
    public bool BackupOnWifiOnly { get; set; } = true;
    public bool IncludeVoiceMemos { get; set; } = true;
    public int MaxBackupCount { get; set; } = 10; // Keep last N backups
}
