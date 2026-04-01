using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MoodTracker.Models;

namespace MoodTracker.Services;

public class SupabaseAuthService : IAuthService
{
    private readonly HttpClient _http;
    private const string SupabaseUrl = "https://your-project.supabase.co";
    private const string SupabaseKey = "your-anon-key";
    
    public bool IsAuthenticated => !string.IsNullOrEmpty(GetSessionToken());
    public User? CurrentUser { get; private set; }

    public SupabaseAuthService()
    {
        _http = new HttpClient { BaseAddress = new Uri($"{SupabaseUrl}/auth/v1/") };
        _http.DefaultRequestHeaders.Add("apikey", SupabaseKey);
        LoadSession();
    }

    // Email/Password Sign Up
    public async Task<AuthResult> SignUpAsync(string email, string password, string name)
    {
        try
        {
            var request = new { email, password, data = new { name } };
            var response = await _http.PostAsJsonAsync("signup", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new AuthResult { Success = false, Error = ParseError(error) };
            }

            var result = await response.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
            if (result?.user != null)
            {
                SaveSession(result.access_token, result.refresh_token, result.expires_in);
                CurrentUser = MapUser(result.user);
                return new AuthResult { Success = true, User = CurrentUser, SessionToken = result.access_token };
            }

            return new AuthResult { Success = false, Error = "Unknown error" };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Signup error: {ex}");
            return new AuthResult { Success = false, Error = ex.Message };
        }
    }

    // Email/Password Sign In
    public async Task<AuthResult> SignInAsync(string email, string password)
    {
        try
        {
            var request = new { email, password };
            var response = await _http.PostAsJsonAsync("token?grant_type=password", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new AuthResult { Success = false, Error = "Invalid email or password" };
            }

            var result = await response.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
            if (result?.user != null)
            {
                SaveSession(result.access_token, result.refresh_token, result.expires_in);
                CurrentUser = MapUser(result.user);
                return new AuthResult { Success = true, User = CurrentUser, SessionToken = result.access_token };
            }

            return new AuthResult { Success = false, Error = "Unknown error" };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Signin error: {ex}");
            return new AuthResult { Success = false, Error = ex.Message };
        }
    }

    // Google OAuth (via WebAuthenticator)
    public async Task<AuthResult> SignInWithGoogleAsync()
    {
#if ANDROID || IOS
        return await SignInWithOAuthAsync("google");
#else
        // Fallback for desktop: open browser auth
        return await SignInWithBrowserOAuthAsync("google");
#endif
    }

    // Apple OAuth (iOS only)
    public async Task<AuthResult> SignInWithAppleAsync()
    {
#if IOS
        return await SignInWithOAuthAsync("apple");
#else
        return new AuthResult { Success = false, Error = "Apple Sign-In only available on iOS" };
#endif
    }

    private async Task<AuthResult> SignInWithOAuthAsync(string provider)
    {
        try
        {
            // Start OAuth flow
            var url = $"{SupabaseUrl}/auth/v1/authorize?provider={provider}&redirect_to=moodtracker://callback";
            
            var authResult = await WebAuthenticator.AuthenticateAsync(
                new Uri(url),
                new Uri("moodtracker://callback"));

            var accessToken = authResult?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
                return new AuthResult { Success = false, Error = "OAuth failed" };

            SaveSession(accessToken, "", 3600);
            await RefreshSessionAsync();
            
            return new AuthResult { Success = true, User = CurrentUser, SessionToken = accessToken };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OAuth error: {ex}");
            return new AuthResult { Success = false, Error = ex.Message };
        }
    }

    private async Task<AuthResult> SignInWithBrowserOAuthAsync(string provider)
    {
        // Desktop: Open browser, user authenticates, copies token
        var url = $"{SupabaseUrl}/auth/v1/authorize?provider={provider}";
        await Launcher.OpenAsync(url);
        return new AuthResult { Success = false, Error = "Please complete sign-in in browser and restart app" };
    }

    public Task SignOutAsync()
    {
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("refresh_token");
        SecureStorage.Remove("token_expiry");
        CurrentUser = null;
        return Task.CompletedTask;
    }

    public async Task<bool> RefreshSessionAsync()
    {
        try
        {
            var refreshToken = await SecureStorage.GetAsync("refresh_token");
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var response = await _http.PostAsJsonAsync("token?grant_type=refresh_token", 
                new { refresh_token = refreshToken });

            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
            if (result?.user != null)
            {
                SaveSession(result.access_token, result.refresh_token, result.expires_in);
                CurrentUser = MapUser(result.user);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Refresh error: {ex}");
        }
        return false;
    }

    public async Task<bool> SendPasswordResetAsync(string email)
    {
