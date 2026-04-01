using SQLite;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoodTracker.Models;

[Table("entries")]
public class Entry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    
    [Range(1, 5)]
    public int Mood { get; set; } = 3;
    
    [Range(1, 10)]
    public int Energy { get; set; } = 5;
    
    public int Sleep { get; set; } = 7; // Hours
    
    // Stored as JSON in SQLite
    public string ActivitiesJson { get; set; } = "[]";
    
    [Ignore]
    public List<string> Activities 
    { 
        get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(ActivitiesJson) ?? new List<string>();
        set => ActivitiesJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
    
    public string Note { get; set; } = string.Empty;
    
    // Voice memo
    public string LocalVoicePath { get; set; } = string.Empty;
    public string VoiceTranscript { get; set; } = string.Empty;
    
    // AI generated
    public string AiSummary { get; set; } = string.Empty;
    public string AiTags { get; set; } = string.Empty; // JSON array
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    
    // Cloud sync status
    public bool IsSyncedToCloud { get; set; } = false;
    public string? CloudEntryId { get; set; }
    
    // Computed properties (not in DB)
    [Ignore]
    public string MoodEmoji => Mood switch
    {
        1 => "😢",
        2 => "😕",
        3 => "😐",
        4 => "🙂",
        5 => "😊",
        _ => "😐"
    };
    
    [Ignore]
    public string MoodLabel => Mood switch
    {
        1 => "Sehr schlecht",
        2 => "Schlecht",
        3 => "Neutral",
        4 => "Gut",
        5 => "Sehr gut",
        _ => "Neutral"
    };
    
    [Ignore]
    public Color MoodColor => Mood switch
    {
        1 => Color.FromArgb("#FF4444"), // Red
        2 => Color.FromArgb("#FF8800"), // Orange
        3 => Color.FromArgb("#FFCC00"), // Yellow
        4 => Color.FromArgb("#88CC00"), // Light Green
        5 => Color.FromArgb("#22AA22"), // Green
        _ => Color.FromArgb("#FFCC00")
    };
    
    [Ignore]
    public string DayName => CreatedAt.ToString("dddd", new System.Globalization.CultureInfo("de-DE"));
    
    [Ignore]
    public string ShortDate => CreatedAt.ToString("dd.MM");
}
