namespace MoodTracker.Views;

public partial class NewEntryPage : ContentPage
{
    public NewEntryPage()
    {
        InitializeComponent();
        Loaded += async (s, e) => {
            if (BindingContext is ViewModels.NewEntryViewModel vm)
                await vm.InitializeAsync();
        };
    }

    private async void OnActivitySelected(object sender, SelectionChangedEventArgs e)
    {
        // Handle activity selection
    }

    private async void OnToggleRecording(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.NewEntryViewModel vm)
            await vm.ToggleRecordingAsync();
    }

    private async void OnGenerateSummary(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.NewEntryViewModel vm)
            await vm.GenerateAiSummaryAsync();
    }

    private async void OnSave(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.NewEntryViewModel vm)
            await vm.SaveAsync();
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.NewEntryViewModel vm)
            await vm.CancelAsync();
    }
}
