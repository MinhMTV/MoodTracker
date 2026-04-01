namespace MoodTracker.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void OnBackupNow(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.SettingsViewModel vm)
            await vm.BackupNowAsync();
    }

    private async void OnExport(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.SettingsViewModel vm)
            await vm.ExportToFileAsync();
    }
}
