using MoodTracker.Models;

namespace MoodTracker.Services;

public interface IAuthService
{
    bool IsAuthenticated { get; }
    User? CurrentUser { get; }
    
    // Email/Password
    Task<AuthResult> SignUpAsync(string email, string password, string name);
    Task<AuthResult> SignInAsync(string email, string password);
    
    // OAuth (Google, Apple)
    Task<AuthResult> SignInWithGoogleAsync();
    Task<AuthResult> SignInWithAppleAsync();
    
    // Session
    Task SignOutAsync();
    Task<bool> RefreshSessionAsync();
    Task<bool> SendPasswordResetAsync(string email);
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public User? User { get; set; }
    public string? SessionToken { get; set; }
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Provider { get; set; } = "email"; // google, apple, email
    public DateTime CreatedAt { get; set; }
}
