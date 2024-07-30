using LibVLCSharp.Shared;

namespace EmyAudio.Services;

public class VlcService : IDisposable
{
    readonly string logFilePath = Path.Combine("Log", "Vlc.log");
    readonly LibVLC libVlc;
    
    public MediaPlayer? mediaPlayer;
    
    public Action<float>? OnPositionChanged { get; set; }
    public Action? OnAudioEnd { get; set; }
    
    public VlcService()
    {
        LibVLCSharp.Shared.Core.Initialize();
        libVlc = new LibVLC();

        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
        File.WriteAllText(logFilePath, "Empty log");
        libVlc.SetLogFile(logFilePath);
        
        Reset();
    }

    public async ValueTask Play(Stream stream)
    {
        var streamMediaInput = new StreamMediaInput(stream);
        var media = new Media(libVlc, streamMediaInput);
        await media.Parse(MediaParseOptions.ParseNetwork);

        mediaPlayer!.Play(media);
    }

    public void Reset()
    {
        if(mediaPlayer is not null)
        {
            var oldMediaPlayer = mediaPlayer;
            oldMediaPlayer.Pause();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                StopOldPlayer().Forget();
                return;

                async Task StopOldPlayer()
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    oldMediaPlayer.Stop();
                }
            });
        }
        
        mediaPlayer = new MediaPlayer(libVlc);
        mediaPlayer!.EndReached += OnAudioEndCallback;
        mediaPlayer!.PositionChanged += OnAudioPositionChangedCallback;
        
    }
    
    void OnAudioPositionChangedCallback(object? sender, MediaPlayerPositionChangedEventArgs e) => OnPositionChanged?.Invoke(e.Position);
    void OnAudioEndCallback(object? sender, EventArgs e) => OnAudioEnd?.Invoke();

    public void Dispose()
    {
        libVlc.Dispose();
        libVlc.CloseLogFile();
    }
}