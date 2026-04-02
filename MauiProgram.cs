using Microsoft.Extensions.Logging;
using MoodTracker.Services;
using MoodTracker.ViewModels;
using MoodTracker.Views;

namespace MoodTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Auth
        builder.Services.AddSingleton<IAuthService, SupabaseAuthService>();
        
        // Services
        builder.Services.AddSingleton<ISqliteDatabaseService, SqliteDatabaseService>();
        builder.Services.AddSingleton<ICloudBackupService, CloudBackupService>();
        builder.Services.AddSingleton<IAiService, OpenRouterAiService>();
        builder.Services.AddSingleton<IVoiceRecordingService, VoiceRecordingService>();
        builder.Services.AddSingleton<IBackupSchedulerService, BackupSchedulerService>();
        builder.Services.AddSingleton<IHapticService, HapticService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<TodayViewModel>();
        builder.Services.AddTransient<TimelineViewModel>();
        builder.Services.AddTransient<StatsViewModel>();
        builder.Services.AddTransient<WeeklyReportViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<NewEntryViewModel>();

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<TodayPage>();
        builder.Services.AddTransient<TimelinePage>();
        builder.Services.AddTransient<StatsPage>();
        builder.Services.AddTransient<WeeklyReportPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<NewEntryPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
