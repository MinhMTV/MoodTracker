namespace MoodTracker.Services;

public interface IHapticService
{
    void SelectionChanged();
    void LightImpact();
    void MediumImpact();
    void HeavyImpact();
    void Success();
    void Warning();
    void Error();
    void Notification(NotificationType type);
    bool IsSupported { get; }
}

public enum NotificationType
{
    Success,
    Warning,
    Error
}

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