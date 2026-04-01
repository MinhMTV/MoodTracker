namespace MoodTracker.Views;

public partial class WeeklyReportPage : ContentPage
{
    public WeeklyReportPage()
    {
        InitializeComponent();
        Loaded += async (s, e) => {
            if (BindingContext is ViewModels.WeeklyReportViewModel vm)
                await vm.LoadReportsAsync();
        };
    }

    private async void OnPreviousWeek(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.WeeklyReportViewModel vm)
            await vm.NavigateToWeekAsync("prev");
    }

    private async void OnNextWeek(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.WeeklyReportViewModel vm)
            await vm.NavigateToWeekAsync("next");
    }

    private async void OnGenerateReport(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.WeeklyReportViewModel vm)
            await vm.RefreshReportAsync();
    }

    private async void OnRefresh(object sender, EventArgs e)
    {
        if (BindingContext is ViewModels.WeeklyReportViewModel vm)
            await vm.RefreshReportAsync();
    }
}
