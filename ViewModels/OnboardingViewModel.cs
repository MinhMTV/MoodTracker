using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace MoodTracker.ViewModels;

public partial class OnboardingViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<OnboardingPageModel> _pages = new();

    [ObservableProperty]
    private int _currentPosition;

    [ObservableProperty]
    private string _actionButtonText = "Weiter";

    public OnboardingViewModel()
    {
        LoadPages();
    }

    private void LoadPages()
    {
        Pages = new ObservableCollection<OnboardingPageModel>
        {
            new()
            {
                Title = "Willkommen bei MoodTracker",
                Description = "Verstehe dich selbst besser. Tagebuch mit smarter KI-Analyse.",
                Illustration = "🧠",
                ActionText = "Los geht's"
            },
            new()
            {
                Title = "Schnell Eintragen",
                Description = "Nur 3 Sekunden für einen Mood-Eintrag. Wähle deinen Emoji, tagge Aktivitäten - fertig.",
                Illustration = "😊",
                ActionText = "Weiter"
            },
            new()
            {
                Title = "KI-Zusammenfassungen",
                Description = "Sprich rein oder schreibe - die KI analysiert deine Einträge und erkennt Muster.",
                Illustration = "🤖",
                ActionText = "Weiter"
            },
            new()
            {
                Title = "Deine Daten bleiben privat",
                Description = "Alle Einträge werden lokal gespeichert. Optionaler Cloud-Backup nur wenn DU willst.",
                Illustration = "🔒",
                ActionText = "Starten"
            }
        };
