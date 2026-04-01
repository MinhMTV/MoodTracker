using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoodTracker.Models;
using MoodTracker.Services;
using System.Collections.ObjectModel;

namespace MoodTracker.ViewModels;

public partial class StatsViewModel : ObservableObject
{
    private readonly ISqliteDatabaseService _database;

    [ObservableProperty]
    private ObservableCollection<Entry> _entries = new();

    [ObservableProperty]
    private double _averageMood;

    [ObservableProperty]
    private double _averageEnergy;

    [ObservableProperty]
    private int _streakDays;

    [ObservableProperty]
    private ObservableCollection<StatItem> _activityStats = new();

    public StatsViewModel(ISqliteDatabaseService database)
    {
        _database = database;
    }

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();
        
        // Load last 30 days
        var from = DateTime.Now.AddDays(-30);
        var to = DateTime.Now;
        entries = await _database.GetEntriesAsync(from, to);
        
        if (entries.Any())
        {
            AverageMood = entries.Average(e => e.Mood);
            AverageEnergy = entries.Average(e => e.Energy);
            
            // Activity stats
            var activities = entries.SelectMany(e => e.Activities).GroupBy(a => a)
                .Select(g => new StatItem { Name = g.Key, Count = g.Count(), Percentage = (double)g.Count() / entries.Count })
                .OrderByDescending(s => s.Count)
                .Take(5);
            ActivityStats = new ObservableCollection<StatItem>(activities);
            
            // Calculate streak
            StreakDays = CalculateStreak();
        }
    }

    private int CalculateStreak()
    {
        var dates = entries.Select(e => e.CreatedAt.Date).Distinct().OrderDescending().ToList();
        if (!dates.Any()) return 0;
        
        int streak = 1;
        DateTime checkDate = dates[0];
        
        for (int i = 1; i < dates.Count; i++)
        {
            if (dates[i] == checkDate.AddDays(-1))
            {
                streak++;
                checkDate = dates[i];
            }
            else break;
        }
        
        return streak;
    }
}

public class StatItem
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage