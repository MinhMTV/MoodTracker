using System.Globalization;

namespace MoodTracker.Converters;

public class MoodToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var currentMood = value is int m ? m : 3;
        var thisMood = parameter is string s && int.TryParse(s, out var p) ? p : 3;
        
        var selected = thisMood == currentMood;
        
        if (selected)
        {
            return thisMood switch
            {
                1 => Color.FromArgb("#FF5252"),
                2 => Color.FromArgb("#FFAB40"),
                3 => Color.FromArgb("#FFD740"),
                4 => Color.FromArgb("#69F0AE"),
                5 => Color.FromArgb("#00E676"),
                _ => Color.FromArgb("#FFD740")
            };
        }
        return Application.Current?.RequestedTheme == AppTheme.Dark 
            ? Color.FromArgb("#2D2D2D") 
            : Color.FromArgb("#F5F5F5");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class MoodToBorderColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var currentMood = value is int m ? m : 3;
        var thisMood = parameter is string s && int.TryParse(s, out var p) ? p : 3;
        
        if (currentMood == thisMood)
        {
            return thisMood switch
            {
                1 => Color.FromArgb("#FF5252"),
                2 => Color.FromArgb("#FFAB40"),
                3 => Color.FromArgb("#FFD740"),
                4 => Color.FromArgb("#69F0AE"),
                5 => Color.FromArgb("#00E676"),
                _ => Color.FromArgb("#FFD740")
            };
        }
        return Color.FromArgb("#E0E0E0");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
