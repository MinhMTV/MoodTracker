namespace MoodTracker.Views;

public partial class TodayPage : ContentPage
{
    public TodayPage()
    {
        InitializeComponent();
        Loaded += async (s, e) => {
            if (BindingContext is ViewModels.TodayViewModel vm)
                await vm.LoadTodayEntryAsync();
        };
    }

    private async void OnPreviousDay(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.TodayViewModel vm)
