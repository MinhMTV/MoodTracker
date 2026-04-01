using SQLite;

namespace MoodTracker.Models;

[Table("activities")]
public class Activity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string Icon { get; set; } = "📝"; // Emoji
    
    public string ColorHex { get; set; } = "#512BD4";
    
    [Ignore]
    public Color DisplayColor => Color.FromArgb(ColorHex);
    
    public int UsageCount { get; set; } = 0;
    
    public bool IsDefault { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUsed { get; set; } = DateTime.UtcNow;
}

// Pre-defined activities
public static class DefaultActivities
{
    public static List<Activity> GetAll() => new()
    {
        new() { Name = "Sport", Icon = "🏃", ColorHex = "#22AA22" },
        new() { Name = "Arbeit", Icon = "💼", ColorHex = "#4488FF" },
        new() { Name = "Freunde", Icon = "👥", ColorHex = "#FF4488" },
        new() { Name = "Familie", Icon = "👨‍👩‍👧‍👦", ColorHex = "#FF8800" },
        new() { Name = "Lesen", Icon = "📚", ColorHex = "#8844FF" },
        new() { Name = "Meditation", Icon = "🧘", ColorHex = "#44CCAA" },
        new() { Name = "Musik", Icon = "🎵", ColorHex = "#CC44AA" },
        new() { Name = "Kochen", Icon = "🍳", ColorHex = "#CC8800" },
        new() { Name = "Spazieren", Icon = "🚶", ColorHex = "#44AA44" },
        new() { Name = "Schlaf", Icon = "😴", ColorHex = "#4444AA" },
        new() { Name = "Gesund", Icon = "🥗", ColorHex = "#22CC44" },
        new() { Name = "Ungesund", Icon = "🍔", ColorHex = "#CC4422" },
    };
}
