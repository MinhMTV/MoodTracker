using MoodTracker.Services;

namespace MoodTracker;

public partial class App : Application
{
    public App(IAuthService authService)
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
