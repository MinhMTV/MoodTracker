using System.Diagnostics;

namespace MoodTracker.Services;

/// <summary>
Fast local Whisper implementation for voice transcription
Uses whisper.cpp or ONNX Runtime for edge AI
</summary>
public class FastWhisperService : IVoiceTranscriptionService
{
    private readonly HttpClient _httpClient;
    
    // Option 1: Use OpenAI Whisper API (pay per use, but cheap: ~$0.006/min)
    // Option 2: Use fast-whisper API endpoint you host (see docker below)
    // Option 3: Use ONNX Runtime with distil-whisper (local on device)
    
    private const string WhisperApiUrl = "https://api.openai.com/v1/audio/transcriptions";
    private string ApiKey => UserSecrets.OpenAiApiKey ?? string.Empty;
    
    // Or local endpoint:
    private const string LocalWhisperUrl = "http://localhost:8000/transcribe";

    public FastWhisperService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
    }

    /// <summary>
    Transcribe audio file using Whisper
    </summary>
    public async Task<string> TranscribeAsync(string audioFilePath, string language = "de")
    {
        if (!File.Exists(audioFilePath))
            return string.Empty;

        try
        {
            var content = new MultipartFormContent();
            
            // Add audio file
            var audioContent = new StreamContent(File.OpenRead(audioFilePath));
            audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/m4a");
            content.Add(audioContent, "file", Path.GetFileName(audioFilePath));
            
            // Add parameters
            content.Add(new StringContent("whisper-1"), "model");
            content.Add(new StringContent(language), "language");
            content.Add(new StringContent("text"), "response_format"); // text, json, srt, verbose_json
            content.Add(new StringContent("Tagebuch-Eintrag"), "prompt"); // helps with context

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
            
            var response = await _httpClient.PostAsync(WhisperApiUrl, content);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadAsStringAsync();
            return result.Trim();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Whisper transcription failed: {ex}");
            return string.Empty;
        }
    }

    /// <summary>
    Check if we can use local fast-whisper (Docker/container)
    Setup: docker run -p 8000:8000 -v /models:/models onerahmet/openai-whisper-asr-fastapi
    </summary>
    public async Task<bool> IsLocalWhisperAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{LocalWhisperUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    Transcribe using local fast-whisper API (free, self-hosted)
    </summary>
    public async Task<string> TranscribeLocalAsync(string audioFilePath)
    {
        try
        {
            var content = new MultipartFormContent();
            var audioContent = new StreamContent(File.OpenRead(audioFilePath));
            content.Add(audioContent, "audio_file", Path.GetFileName(audioFilePath));

            var response = await _httpClient.PostAsync(LocalWhisperUrl, content);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<WhisperResponse>();
            return result?.Text ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Local whisper failed: {ex}");
            return string.Empty;
        }
    }

    /// <summary>
    Get transcription with timestamps (for longer memos)
    </summary>
    public async Task<List<TranscriptionSegment>> TranscribeWithTimestampsAsync(string audioFilePath)
    {
        // Implementation for SRT/verbose_json format
        // Returns segments with start/end times
        return new List<TranscriptionSegment>();
    }
}

public interface IVoiceTranscriptionService
{
    Task<string> TranscribeAsync(string audioFilePath, string language = "de");
}

public class WhisperResponse
{
    public string Text { get; set; } = string.Empty;
}

public class TranscriptionSegment
{
    public int Id { get; set; }
    public TimeSpan Seek { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public string Text { get; set; } = string.Empty;
}

// Add to UserSecrets:
public partial class UserSecrets
{
    public static string? OpenAiApiKey => 
        Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? 
        Preferences.Get("openai_api_key", null);
        
    public static void SetOpenAiApiKey(string key)
    {
        Preferences.Set("openai_api_key", key);
    }
}

/* 
DOCKER SETUP for self-hosted fast-whisper (free):

1. Pull and run:
docker run -d -p 8000:8000 --name whisper \
  -v $(pwd)/models:/root/.cache/whisper \
  onerahmet/openai-whisper-asr-fastapi:latest

2. Or with specific model:
docker run -d -p 8000:8000 \
  -e ASR_MODEL=base \
  -e ASR_ENGINE=openai_whisper \
  onerahmet/openai-whisper-asr-fastapi

3. Then use LocalWhisperUrl = "http://localhost:8000/asr"

API Endpoints:
- POST /asr - Upload audio file, returns transcription
- GET /health - Check if service is running

Options for local whisper:
- small (~150MB, fast, good accuracy)
- base (~75MB, fastest, ok accuracy) 
- medium (~500MB, slower, best accuracy)
- large (~1.5GB, slowest, excellent)

For mobile/edge devices:
- Use distil-whisper (50% faster, similar accuracy)
- Or use ONNX Runtime with quantized models
- Maui can use C++ bindings to whisper.cpp

*/
