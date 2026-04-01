using MoodTracker.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace MoodTracker.Services;

public class OpenRouterAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://openrouter.ai/api/v1/";
    private string ApiKey => UserSecrets.OpenRouterApiKey ?? string.Empty;

    public OpenRouterAiService()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl), Timeout = TimeSpan.FromSeconds(30) };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://moodtracker.app");
        _httpClient.DefaultRequestHeaders.Add("X-Title", "MoodTracker");
    }

    public Task<bool> IsAvailableAsync() => Task.FromResult(!string.IsNullOrEmpty(ApiKey));

    public async Task<string> SummarizeEntryAsync(Entry entry)
    {
        var note = !string.IsNullOrWhiteSpace(entry.Note) ? entry.Note : "(keine Notiz)";
        var activities = entry.Activities.Any() ? string.Join(", ", entry.Activities) : "(keine Aktivitäten)";
        
        var prompt = $"""Fasse kurz zusammen (max. 2 Sätze): Tagebucheintrag vom {entry.CreatedAt:dd.MM}. Stimmung: {entry.MoodEmoji} ({entry.Mood}/5). Energie: {entry.Energy}/10. Aktivitäten: {activities}. Notiz: {note}""";
        
        return await CallAiAsync(prompt, 150);
    }

    public async Task<WeeklyReport> GenerateWeeklyReportAsync(List<Entry> entries, DateTime weekStart)
    {
        if (entries.Count == 0)
            return new WeeklyReport { WeekStart = weekStart, Summary = "Keine Einträge diese Woche." };

        var weekEnd = weekStart.AddDays(6);
        var avgMood = entries.Average(e => e.Mood);
        var avgEnergy = entries.Average(e => e.Energy);
        var trend = CalculateTrend(entries);

        var prompt = $"""Woche {weekStart:dd.MM} - {weekEnd:dd.MM}. {entries.Count} Einträge. Ø Stimmung {avgMood:F1}/5, Ø Energie {avgEnergy:F1}/10. Trend: {trend}. Erstelle JSON: summary(2 Sätze empathisch), patterns(3 Zusammenhänge als Array), coachingTip(kurz), moodTrend""";

        try
        {
            var response = await CallAiAsync(prompt, 400);
            var parsed = JsonConvert.DeserializeObject<dynamic>(response);
            
            return new WeeklyReport
            {
                WeekStart = weekStart, WeekEnd = weekEnd,
                Summary = parsed?.summary?.ToString() ?? $"Diese Woche: Ø Stimmung {avgMood:F1}/5.",
                Patterns = parsed?.patterns?.ToObject<List<string>>() ?? new List<string>(),
                CoachingTip = parsed?.coachingTip?.ToString() ?? "Bleibe achtsam.",
                MoodTrend = parsed?.moodTrend?.ToString() ?? trend,
                AverageMood = avgMood, AverageEnergy = avgEnergy, EntryCount = entries.Count
            };
        }
        catch
        {
            return new WeeklyReport
            {
                WeekStart = weekStart, WeekEnd = weekEnd,
                Summary = $"Diese Woche: Ø Stimmung {avgMood:F1}/5, Ø Energie {avgEnergy:F1}/10.",
                Patterns = new List<string>(), MoodTrend = trend,
                CoachingTip = "Bemerke, was deine Stimmung beeinflusst.",
                AverageMood = avgMood, AverageEnergy = avgEnergy, EntryCount = entries.Count
            };
        }
    }

    private string CalculateTrend(List<Entry> entries)
    {
        if (entries.Count < 2) return "stable";
        var mid = entries.Count / 2;
        var first = entries.Take(mid).Average(e => e.Mood);
        var second = entries.Skip(mid).Average(e => e.Mood);
        return second > first + 0.3 ? "improving" : second < first - 0.3 ? "declining" : "stable";
    }

    public async Task<List<string>> ExtractTagsAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();
        var prompt = $"""Extrahiere 3-5 Tags aus: \"{text}\" JSON: {{"tags":["tag1","tag2",...]}}""";
        try
        {
            var response = await CallAiAsync(prompt, 100);
            var parsed = JsonConvert.DeserializeObject<dynamic>(response);
            return parsed?.tags?.ToObject<List<string>>() ?? new List<string>();
        }
        catch { return new List<string>(); }
    }

    public async Task<string> GetCoachingTipAsync(List<Entry> recentEntries)
    {
        var avg = recentEntries.Any() ? recentEntries.Average(e => e.Mood) : 3;
        var prompt = $"""Stimmung Ø {avg:F1}/5. Gib 1-2 empathische Coaching-Sätze.""";
        return await CallAiAsync(prompt, 150);
    }

    public async Task<string> TranscribeVoiceAsync(byte[] audioData)
    {
        return await Task.FromResult("[Sprachnachricht transkribiert]");
    }

    private async Task<string> CallAiAsync(string prompt, int maxTokens = 300, string model = "openrouter/xiaomi/mimo-v2-pro")
    {
        var request = new
        {
            model = model,
            messages = new[] { new { role = "system", content = "Du bist ein empathischer Tagebuch-Assistent. Antworte kurz, wärmend und konstruktiv auf Deutsch." }, new { role = "user", content = prompt } },
            max_tokens = maxTokens
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("chat/completions", content);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadAsStringAsync();
        var parsed = JsonConvert.DeserializeObject<dynamic>(result);
        return parsed?.choices?[0]?.message?.content?.ToString()?.Trim() ?? string.Empty;
    }
}
