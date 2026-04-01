using MoodTracker.Models;

namespace MoodTracker.Services;

public interface IAiService
{
    Task<string> SummarizeEntryAsync(Entry entry);
    Task<WeeklyReport> GenerateWeeklyReportAsync(List<Entry> entries, DateTime weekStart);
    Task<List<string>> ExtractTagsAsync(string text);
    Task<string> GetCoachingTipAsync(List<Entry> recentEntries);
    Task<string> TranscribeVoiceAsync(byte[] audioData);
    Task<bool> IsAvailableAsync();
}
