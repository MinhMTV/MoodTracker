namespace MoodTracker.Services;

public interface IVoiceRecordingService
{
    Task<bool> RequestPermissionsAsync();
    Task StartRecordingAsync();
    Task<string> StopRecordingAsync();
    Task<string> GetRecordingPathAsync();
    Task<bool> IsRecordingAsync();
    Task DeleteRecordingAsync(string path);
    Task<byte[]> GetAudioBytesAsync(string path);
}

public class VoiceRecordingService : IVoiceRecordingService
{
    public virtual Task<bool> RequestPermissionsAsync() => Task.FromResult(true);
    public virtual Task StartRecordingAsync() => Task.CompletedTask;
    public virtual Task<string> StopRecordingAsync() => Task.FromResult(string.Empty);
    public virtual Task<string> GetRecordingPathAsync() => Task.FromResult(string.Empty);
    public virtual Task<bool> IsRecordingAsync() => Task.FromResult(false);
    public virtual Task DeleteRecordingAsync(string path) => Task.CompletedTask;
    public virtual Task<byte[]> GetAudioBytesAsync(string path) => Task.FromResult(Array.Empty<byte>());
}
