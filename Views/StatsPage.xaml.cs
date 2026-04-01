namespace MoodTracker.Views;

public partial class StatsPage : ContentPage
{
    public StatsPage()
    {
        InitializeComponent();
        Loaded += async (s, e) => {
            if (BindingContext is ViewModels.StatsViewModel vm)
                await vm.InitializeAsync();
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.StatsViewModel vm)
            await vm.InitializeAsync();
    }
}
