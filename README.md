# MoodTracker

Cross-platform Mood Journal built with .NET MAUI. 
Like Daylio, but with AI-powered insights.

## Features

| Feature | Status |
|---------|--------|
| Mood tracking with emoji | ✅ |
| Energy & sleep tracking | ✅ |
| Activity tagging | ✅ |
| Voice memos + AI transcription | ✅ (Fast Whisper ready) |
| AI daily summaries (OpenRouter) | ✅ |
| Weekly AI reports + coaching | ✅ |
| Statistics & charts | ✅ |
| Local SQLite storage | ✅ |
| Cloud backup (iCloud/Google Drive) | ✅ |
| User profile / AI memory | ✅ |

## Tech Stack

- **UI**: .NET MAUI + XAML
- **Database**: SQLite (sqlite-net-pcl)
- **Charts**: Microcharts
- **AI**: OpenRouter (Xiaomi MIMO → Kimi fallback)
- **Voice**: Fast Whisper (local or OpenAI API)
- **Backup**: Platform-native (iCloud / Google Drive)

## Quick Start

```bash
# Clone (when public)
git clone https://github.com/YOURUSER/MoodTracker.git
cd MoodTracker

# Restore packages
dotnet restore

# Build & run
dotnet build -t:Run -f net8.0-android
```

## Screenshots

[Coming soon]

## Setup

1. Get free OpenRouter key: https://openrouter.ai/keys
2. Set key in Settings → "sk-or-xxxxx"
3. (Optional) Fast Whisper: `docker run -p 8000:8000 onerahmet/openai-whisper-asr-fastapi`

## Project Structure

```
MoodTracker/
├── Models/              # Entry, Activity, WeeklyReport, UserProfile
├── Services/            # Database, AI, Backup, Voice (DI ready)
├── ViewModels/          # MVVM with CommunityToolkit.Mvvm
├── Views/               # XAML pages
├── Converters/          # IValueConverters
└── Resources/           # Styles, Colors
```

## Customization

Edit `UserProfile` for personalized AI:
- Mental health history
- Known triggers
- Coping strategies
- Current goals

AI uses this context for relevant coaching tips.

## License

MIT
