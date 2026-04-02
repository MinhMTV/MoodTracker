using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using MoodTracker.Models;
using MoodTracker.Services;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace MoodTracker.ViewModels;

public partial class StatsViewModel : ObservableObject
{
    private readonly ISqliteDatabaseService _database;
    private int _periodDays = 30;

    [ObservableProperty]
    private int _entriesCount;

    [ObservableProperty]
    private double _averageMood;

    [ObservableProperty]
    private double _averageEnergy;

    [ObservableProperty]
    private int _streakDays;

    [ObservableProperty]
    private Color _averageMoodColor = Colors.Gray;

    [ObservableProperty]
    private Chart? _moodChart;

    [ObservableProperty]
    private Chart? _activityChart;

    [ObservableProperty]
    private ObservableCollection<ActivityStat> _activityStats = new();

    [ObservableProperty]
    private bool _hasActivityData;

    public StatsViewModel(ISqliteDatabaseService database)
    {
        _database = database;
    }

    public async Task InitializeAsync()
    {
        await LoadStatsAsync();
    }

    [RelayCommand]
    private async Task SetPeriodAsync(string days)
    {
        if (int.TryParse(days, out _periodDays))
            await LoadStatsAsync();
    }

    private async Task LoadStatsAsync()
    {
        await _database.InitializeAsync();
        
        var from = DateTime.Now.AddDays(-_periodDays);
        var to = DateTime.Now;
        var entries = await _database.GetEntriesAsync(from, to);

        EntriesCount = entries.Count;

        if (entries.Any())
        {
            AverageMood = entries.Average(e => e.Mood);
            AverageEnergy = entries.Average(e => e.Energy);
            AverageMoodColor = GetMoodColor((int)Math.Round(AverageMood));
            
            SetupMoodChart(entries);
            SetupActivityChart(entries);
            CalculateStreak(entries);
        }
    }

    private void SetupMoodChart(List<Entry> entries)
    {
        var entriesByDay = entries
            .GroupBy(e => e.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new { Date = g.Key, AvgMood = g.Average(e => e.Mood) })
            .ToList();

        var chartEntries = new List<ChartEntry>();
        var moodColors = new[] { 
            SKColor.Parse("#FF5252"), SKColor.Parse("#FFAB40"), SKColor.Parse("#FFD740"), 
            SKColor.Parse("#69F0AE"), SKColor.Parse("#00E676") 
        };

        foreach (var day in entriesByDay.TakeLast(14)) // Show last 14 days
        {
            var colorIndex = Math.Max(0, Math.Min(4, (int)day.AvgMood - 1));
            chartEntries.Add(new ChartEntry((float)day.AvgMood)
            {
                Label = day.Date.ToString("dd."),
                ValueLabel = day.AvgMood.ToString("F1"),
                Color = moodColors[colorIndex]
            });
        }

        MoodChart = new LineChart
        {
            Entries = chartEntries,
            LineMode = LineMode.Spline,
            LineSize = 4,
            PointMode = PointMode.Circle,
            PointSize = 10,
            MinValue = 1,
            MaxValue = 5,
            LabelTextSize = 28,
            BackgroundColor = SKColors.Transparent,
            Margin = 20
        };
    }

    private void SetupActivityChart(List<Entry> entries)
    {
        var activityCounts = entries
            .SelectMany(e => e.Activities)
            .GroupBy(a => a)
            .OrderByDescending(g => g.Count())
            .Take(6)
            .ToList();

        if (!activityCounts.Any())
        {
            HasActivityData = false;
            return;
        }

        HasActivityData = true;
        var chartColors = new[] { "#512BD4", "#FF4081", "#00C853", "#FFAB40", "#448AFF", "#69F0AE" };
        
        var chartEntries = activityCounts.Select((g, i) => new ChartEntry(g.Count())
        {
            Label = g.Key.Length > 8 ? g.Key[..8] : g.Key,
            ValueLabel = g.Count().ToString(),
            Color = SKColor.Parse(chartColors[i % chartColors.Length])
        }).ToList();

        ActivityChart = new DonutChart
        {
            Entries = chartEntries,
            HoleRadius = 0.5f,
            LabelTextSize = 32,
            BackgroundColor = SKColors.Transparent,
            Margin = 20
        };

        // Also populate list
        ActivityStats = new ObservableCollection<ActivityStat>(activityCounts.Select(g => new ActivityStat
        {
            Name = g.Key,
            Count = g.Count(),
            Percentage = (double)g.Count() / entries.Count,
            Icon = GetActivityIcon(g.Key)
        }));
    }

    private void CalculateStreak(List<Entry> entries)
    {
        var dates = entries.Select(e => e.CreatedAt.Date).Distinct().OrderDescending().ToList();
        if (!dates.Any())
        {
            StreakDays = 0;
            return;
        }

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
        
        StreakDays = streak;
    }

    private Color GetMoodColor(int mood) => mood switch
    {
        1 => Color.FromArgb("#FF5252"),
        2 => Color.FromArgb("#FFAB40"),
        3 => Color.FromArgb("#FFD740"),
        4 => Color.FromArgb("#69F0AE"),
        5 => Color.FromArgb("#00E676"),
        _ => Color.FromArgb("#FFD740")
    };

    private string GetActivityIcon(string activity) => activity.ToLower() switch
    {
        var s when s.Contains("sport") => "🏃",
        var s when s.Contains("arbeit") => "💼",
        var s when s.Contains("freunde") => "👥",
        var s when s.Contains("familie") => "👨‍👩‍👧‍",
        var s when s.Contains("lesen") => "📚",
        var s when s.Contains("meditation") => "🧘",
        var s when s.Contains("musik") => "🎵",
        var s when s.Contains("schlaf") => "😴",
        _ => "📝"
    };
}

public partial class ActivityStat : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _count;

    [ObservableProperty]
    private double _percentage;

    [ObservableProperty]
    private string _icon = "📝";
}
