namespace MoodTracker.Services;

public static class UserSecrets
{
    // Store API keys securely in production
    // For development, you can set these via environment variables
    // or use .NET User Secrets: dotnet user-secrets set "OpenRouterApiKey" "your-key"
    
    public static string? OpenRouterApiKey => 
        Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? 
        Preferences.Get("openrouter_api_key", null);
    
    public static void SetOpenRouterApiKey(string key)
    {
        Preferences.Set("openrouter_api_key", key);
    }
}
