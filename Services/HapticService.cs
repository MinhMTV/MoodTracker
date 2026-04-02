namespace MoodTracker.Services;

public class HapticService : IHapticService
{
    public bool IsSupported => true;

    public void SelectionChanged()
    {
        TryExecute(() => HapticFeedback.Perform(HapticFeedbackType.Click));
    }

    public void LightImpact()
    {
        TryExecute(() => HapticFeedback.Perform(HapticFeedbackType.Click));
    }

    public void MediumImpact()
    {
        TryExecute(() => HapticFeedback.Perform(HapticFeedbackType.LongPress));
    }

    public void HeavyImpact()
    {
        TryExecute(() => HapticFeedback.Perform(HapticFeedbackType.LongPress));
    }

    public void Success()
    {
        TryExecute(() => HapticFeedback.Perform(HapticFeedbackType.LongPress));
    }

    public void Warning()
    {
        TryExecute(() => HapticFeedback.Perform(HapticFeedbackType.Click));
    }

    public void Error()
    {
        TryExecute(() => HapticFeedback.Perform(HapticFeedbackType.LongPress));
    }

    public void Notification(NotificationType type)
    {
        var hapticType = type switch
        {
            NotificationType.Success => HapticFeedbackType.LongPress,
            NotificationType.Warning => HapticFeedbackType.Click,
            NotificationType.Error => HapticFeedbackType.LongPress,
            _ => HapticFeedbackType.Click
        };
        TryExecute(() => HapticFeedback.Perform(hapticType));
    }

    private void TryExecute(Action action)
    {
        try
        {
            if (IsSupported)
                action();
        }
        catch { /* Silent fail */ }
    }
}
