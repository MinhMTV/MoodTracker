using SQLite;

namespace MoodTracker.Models;

[Table("weekly_reports")]
public class WeeklyReport
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    
    public DateTime WeekStart { get; set; }
    
    public DateTime WeekEnd { get; set; }
    
    // AI generated content
    public string Summary { get; set; } = string.Empty;
    
    public string PatternsJson { get; set; } = "[]"; // JSON array of detected patterns
    
    [Ignore]
    public List<string> Patterns
    {
        get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(PatternsJson) ?? new List<string>();
        set => PatternsJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
    
    public string CoachingTip { get; set; } = string.Empty;
    
    public string MoodTrend { get; set; } = string.Empty; // "improving", "stable", "declining"
    
    // Stats
    public double AverageMood { get; set; }
    public double AverageEnergy { get; set; }
    public int EntryCount { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    [Ignore]
    public string WeekLabel => $"KW {ISOWeek.GetWeekOfYear(WeekStart)} · {WeekStart:dd.MM} - {WeekEnd:dd.MM}";
}

public static class ISOWeek
{
    public static int GetWeekOfYear(DateTime date)
    {
        var day = (int)date.DayOfWeek;
        if (day == 0) day = 7;
        var thursday = date.AddDays(4 - day);
        return (thursday.DayOfYear - 1) / 7 + 1;
    }
}
