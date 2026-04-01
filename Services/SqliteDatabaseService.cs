using SQLite;
using MoodTracker.Models;

namespace MoodTracker.Services;

public class SqliteDatabaseService : ISqliteDatabaseService
{
    private SQLiteAsyncConnection? _database;
    private bool _isInitialized = false;

    private async Task Init()
    {
        if (_isInitialized) return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "moodtracker.db");
        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<Entry>();
        await _database.CreateTableAsync<Activity>();
        await _database.CreateTableAsync<WeeklyReport>();

        // Seed default activities if empty
        var existingActivities = await _database.Table<Activity>().CountAsync();
        if (existingActivities == 0)
        {
            var defaults = DefaultActivities.GetAll();
            foreach (var activity in defaults)
            {
                activity.IsDefault = true;
                await _database.InsertAsync(activity);
            }
        }

        _isInitialized = true;
    }

    public async Task InitializeAsync() => await Init();

    public async Task CloseAsync()
    {
        if (_database != null)
        {
            await _database.CloseAsync();
            _database = null;
            _isInitialized = false;
        }
    }

    // Entries
    public async Task<Entry> SaveEntryAsync(Entry entry)
    {
        await Init();
        entry.LastModified = DateTime.UtcNow;
        entry.IsSyncedToCloud = false;

        if (entry.Id == 0)
        {
            await _database!.InsertAsync(entry);
        }
        else
        {
            await _database!.UpdateAsync(entry);
        }
        return entry;
    }

    public async Task<Entry?> GetEntryByIdAsync(int id)
    {
        await Init();
        return await _database!.Table<Entry>().Where(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Entry>> GetEntriesAsync(DateTime from, DateTime to)
    {
        await Init();
        return await _database!.Table<Entry>()
            .Where(e => e.CreatedAt >= from && e.CreatedAt < to)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Entry>> GetEntriesForWeekAsync(DateTime weekStart)
    {
        var weekEnd = weekStart.AddDays(7);
        return await GetEntriesAsync(weekStart, weekEnd);
    }

    public async Task<List<Entry>> GetEntriesForMonthAsync(int year, int month)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1);
        return await GetEntriesAsync(from, to);
    }

    public async Task<Entry?> GetEntryForDateAsync(DateTime date)
    {
        await Init();
        var from = date.Date;
        var to = from.AddDays(1);
        return await _database!.Table<Entry>()
            .Where(e => e.CreatedAt >= from && e.CreatedAt < to)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteEntryAsync(int id)
    {
        await Init();
        await _database!.DeleteAsync<Entry>(id);
    }

    public async Task<int> GetEntryCountAsync()
    {
        await Init();
        return await _database!.Table<Entry>().CountAsync();
    }

    // Activities
    public async Task<List<Activity>> GetActivitiesAsync()
    {
        await Init();
        return await _database!.Table<Activity>()
            .OrderByDescending(a => a.UsageCount)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }

    public async Task SaveActivityAsync(Activity activity)
    {
        await Init();
        if (activity.Id == 0)
            await _database!.InsertAsync(activity);
        else
            await _database!.UpdateAsync(activity);
    }

    public async Task DeleteActivityAsync(int id)
    {
        await Init();
        var activity = await _database!.Table<Activity>().Where(a => a.Id == id).FirstOrDefaultAsync();
        if (activity != null && !activity.IsDefault)
        {
            await _database.DeleteAsync<Activity>(id);
        }
    }

    public async Task IncrementActivityUsageAsync(string activityName)
    {
        await Init();
        var activity = await _database!.Table<Activity>().Where(a => a.Name == activityName).FirstOrDefaultAsync();
        if (activity != null)
        {
            activity.UsageCount++;
            activity.LastUsed = DateTime.UtcNow;
            await _database.UpdateAsync(activity);
        }
    }

    // Weekly Reports
    public async Task<WeeklyReport> SaveWeeklyReportAsync(WeeklyReport report)
    {
        await Init();
        if (report.Id == 0)
            await _database!.InsertAsync(report);
        else
            await _database!.UpdateAsync(report);
        return report;
    }

    public async Task<WeeklyReport?> GetWeeklyReportAsync(DateTime weekStart)
    {
        await Init();
        return await _database!.Table<WeeklyReport>()
            .Where(r => r.WeekStart == weekStart)
            .FirstOrDefaultAsync();
    }

    public async Task<List<WeeklyReport>> GetWeeklyReportsAsync(int count = 10)
    {
        await Init();
        return await _database!.Table<WeeklyReport>()
            .OrderByDescending(r => r.WeekStart)
            .Take(count)
            .ToListAsync();
    }

    public async Task DeleteWeeklyReportAsync(int id)
    {
        await Init();
        await _database!.DeleteAsync<WeeklyReport>(id);
    }

    // Sync
    public async Task<List<Entry>> GetUnsyncedEntriesAsync()
    {
        await Init();
        return await _database!.Table<Entry>().Where(e => !e.IsSyncedToCloud).ToListAsync();
    }

    public async Task MarkEntriesSyncedAsync(List<int> entryIds)
    {
        await Init();
        foreach (var id in entryIds)
        {
            var entry = await GetEntryByIdAsync(id);
            if (entry != null)
            {
                entry.IsSyncedToCloud = true;
                await _database!.UpdateAsync(entry);
            }
        }
    }

    public async Task<List<Entry>> GetAllEntriesAsync()
    {
        await Init();
        return await _database!.Table<Entry>().OrderByDescending(e => e.CreatedAt).ToListAsync();
    }
}
