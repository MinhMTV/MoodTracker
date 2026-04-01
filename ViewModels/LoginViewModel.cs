using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoodTracker.Services;

namespace MoodTracker.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty; // For registration

    [ObservableProperty]
    private bool _isRegistering;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Email und Passwort eingeben");
            return;
        }

        IsLoading = true;
        HasError = false;

        var result = await _authService.SignInAsync(Email, Password);
        IsLoading = false;

        if (result.Success)
            await Shell.Current.GoToAsync("//main");
        else
            ShowError(result.Error ?? "Login fehlgeschlagen");
    }

    [RelayCommand]
    private async Task SignUpAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Alle Felder ausfüllen");
            return;
        }

        if (Password.Length < 6)
        {
            ShowError("Passwort mindestens 6 Zeichen");
            return;
        }

        IsLoading = true;
        HasError = false;

        var result = await _authService.SignUpAsync(Email, Password, Name);
        IsLoading = false;

        if (result.Success)
            await Shell.Current.GoToAsync("//main");
        else
            ShowError(result.Error ?? "Registrierung fehlgeschlagen");
    }

    [RelayCommand]
    private async Task SignInWithGoogleAsync()
    {
        IsLoading = true;
        var result = await _authService.SignInWithGoogleAsync();
        IsLoading = false;

        if (result.Success)
            await Shell.Current.GoToAsync("//main");
        else
            ShowError(result.Error ?? "Google Login fehlgeschlagen");
    }

    [RelayCommand]
    private async Task SignInWithAppleAsync()
    {
        IsLoading = true;
        var result = await _authService.SignInWithAppleAsync();
        IsLoading = false;

        if (result.Success)
            await Shell.Current.GoToAsync("//main");
        else
            ShowError(result.Error ?? "Apple Login fehlgeschlagen");
    }

    [RelayCommand]
    private void ToggleMode() => IsRegistering = !IsRegistering;

    [RelayCommand]
    private async Task ContinueOfflineAsync()
    {
        // Allow using app without account (local only)
        Preferences.Set("offline_mode", true);
        await Shell.Current.GoToAsync("//main");
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }
}
