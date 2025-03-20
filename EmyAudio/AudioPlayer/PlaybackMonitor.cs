using JetBrains.Annotations;
using Silk.NET.OpenAL;

namespace EmyAudio.AudioPlayer;

public class PlaybackMonitor : IDisposable
{
    readonly AL al;
    readonly uint audioSource;
    readonly Timer playbackMonitor;

    float lastPosition;
    
    public event Action? OnAudioEnd;
    public event Action<float>? OnPositionChanged;

    [PublicAPI]
    public TimeSpan RefreshRate { get; set; } = TimeSpan.FromMilliseconds(100);

    public PlaybackMonitor(AL al, uint audioSource)
    {
        this.al = al;
        this.audioSource = audioSource;

        // Infinite dueTime and period so it don't start until we tell it to.
        playbackMonitor = new Timer(MonitorPlayback, null, Timeout.Infinite, Timeout.Infinite);
    }
    
    void MonitorPlayback(object? _)
    {
        HasAudioEnded();
        HasPositionChanged();
    }

    void HasAudioEnded()
    {
        al.GetSourceProperty(audioSource, GetSourceInteger.SourceState, out var sourceState);
        if (sourceState != (int)SourceState.Stopped)
            return;
        
        OnAudioEnd?.Invoke();
    }

    void HasPositionChanged()
    {
        al.GetSourceProperty(audioSource, SourceFloat.SecOffset, out var currentPosition);
        if (!(Math.Abs(currentPosition - lastPosition) > 0.1f))// Threshold to avoid too frequent updates
            return;
        lastPosition = currentPosition;
        OnPositionChanged?.Invoke(currentPosition);

    }

    public void Start() => playbackMonitor.Change(0, RefreshRate.Milliseconds);
    public void Stop() => playbackMonitor.Change(Timeout.Infinite, Timeout.Infinite);
    
    public void Dispose() => playbackMonitor.Dispose();
}
