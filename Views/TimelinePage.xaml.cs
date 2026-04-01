namespace MoodTracker.Views;

public partial class TimelinePage : ContentPage
{
    public TimelinePage()
    {
        InitializeComponent();
        Loaded += async (s, e) => {
            if (BindingContext is ViewModels.TimelineViewModel vm)
                await vm.LoadEntriesAsync();
        };
    }

    private async void OnPreviousMonth(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.TimelineViewModel vm)
            await vm.NavigateToPreviousMonthAsync();
    }

    private async void OnNextMonth(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.TimelineViewModel vm)
            await vm.NavigateToNextMonthAsync();
    }

    private async void OnEntryTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Models.Entry entry && BindingContext is ViewModels.TimelineViewModel vm)
            await vm.OpenEntryAsync(entry);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.TimelineViewModel vm)
            await vm.LoadEntriesAsync();
    }
}
