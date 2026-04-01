using MoodTracker.Models;

namespace MoodTracker.Services;

public interface ISqliteDatabaseService
{
    Task InitializeAsync();
    Task CloseAsync();
    
    // Entries
    Task<Entry> SaveEntryAsync(Entry entry);
    Task<Entry?> GetEntryByIdAsync(int id);
    Task<List<Entry>> GetEntriesAsync(DateTime from, DateTime to);
    Task<List<Entry>> GetEntriesForWeekAsync(DateTime weekStart);
    Task<List<Entry>> GetEntriesForMonthAsync(int year, int month);
    Task<Entry?> GetEntryForDateAsync(DateTime date);
    Task DeleteEntryAsync(int id);
    Task<int> GetEntryCountAsync();
    
    // Activities
    Task<List<Activity>> GetActivitiesAsync();
    Task SaveActivityAsync(Activity activity);
    Task DeleteActivityAsync(int id);
    Task IncrementActivityUsageAsync(string activityName);
    
    // Weekly Reports
    Task<WeeklyReport> SaveWeeklyReportAsync(WeeklyReport report);
    Task<WeeklyReport?> GetWeeklyReportAsync(DateTime weekStart);
    Task<List<WeeklyReport>> GetWeeklyReportsAsync(int count = 10);
    Task DeleteWeeklyReportAsync(int id);
    
    // Sync
    Task<List<Entry>> GetUnsyncedEntriesAsync();
    Task MarkEntriesSyncedAsync(List<int> entryIds);
    Task<List<Entry>> GetAllEntriesAsync(); // For backup
}
