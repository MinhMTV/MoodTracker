using SQLite;

namespace MoodTracker.Models;

/// <summary>
User Profile / Memory Store - like memory.md for the AI
</summary>
[Table("user_profile")]
public class UserProfile
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    // Core Identity
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Pronouns { get; set; } = string.Empty; // er/sie/they/etc
    
    // Life Context
    public string Occupation { get; set; } = string.Empty; // Job/Schüler/Student
    public string RelationshipStatus { get; set; } = string.Empty;
    public string LivingSituation { get; set; } = string.Empty; // alleine, WG, Familie
    
    // Health Context
    public string MentalHealthHistory { get; set; } = string.Empty; // Depression, Angst, ADHD, etc
    public string Medications { get; set; } = string.Empty;
    public string Therapist { get; set; } = string.Empty; // ja/nein/name
    
    // Triggers & Patterns (AI uses this for context)
    public string KnownTriggers { get; set; } = string.Empty; // JSON array
    public string CopingStrategies { get; set; } = string.Empty; // was hilft
    public string SleepPattern { get; set; } = string.Empty; // eher Nachteule/Lerche
    public string Stressors { get; set; } = string.Empty; // aktuelle Stressfaktoren
    
    // Goals & Values
    public string CurrentGoals { get; set; } = string.Empty;
    public string CoreValues { get; set; } = string.Empty; // Familie, Karriere, Gesundheit, etc
    
    // AI Preferences
    public int CoachingStyle { get; set; } = 2; // 1=direkt, 2=empathisch, 3=analytisch, 4=spirituell
    public bool UseAiMemory { get; set; } = true; // AI remembers past entries?
    public int AiContextWindow { get; set; } = 7; // wie viele Tage zurück schaut AI
    
    // Saved for AI context
    public string LifeSummary { get; set; } = string.Empty; // 2-3 Sätze über den User
    public string AiInstructions { get; set; } = string.Empty; // custom prompts
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Helper for AI prompt building
    public string GetContextPrompt()
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrEmpty(Name)) parts.Add($"Name: {Name}");
        if (!string.IsNullOrEmpty(Pronouns)) parts.Add($"Pronomen: {Pronouns}");
        if (Age > 0) parts.Add($"{Age} Jahre alt");
        if (!string.IsNullOrEmpty(Occupation)) parts.Add($"Beruf/Situation: {Occupation}");
        if (!string.IsNullOrEmpty(MentalHealthHistory)) parts.Add($"Vorgeschichte: {MentalHealthHistory}");
        if (!string.IsNullOrEmpty(KnownTriggers)) parts.Add($"Bekannte Trigger: {KnownTriggers}");
        if (!string.IsNullOrEmpty(CopingStrategies)) parts.Add($"Hilfreiche Strategien: {CopingStrategies}");
        if (!string.IsNullOrEmpty(CurrentGoals)) parts.Add($"Aktuelle Ziele: {CurrentGoals}");
        if (!string.IsNullOrEmpty(LifeSummary)) parts.Add($"Über mich: {LifeSummary}");
        
        var style = CoachingStyle switch
        {
            1 => "