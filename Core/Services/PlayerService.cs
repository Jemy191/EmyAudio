using Core.Models;
using LibVLCSharp.Shared;

namespace Core.Services;

public class PlayerService : IDisposable
{
    readonly YoutubeStreamingService youtubeStreamingService;
    readonly VlcService vlcService;
    readonly DownloadedAudioService downloadedAudioService;
    readonly SettingsService settingsService;

    bool started;
    PlayerCallbacks? callbacks;
    RuntimePlaylist? playlist;
    AudioInfo? currentAudioInfo;

    public PlayerService(YoutubeStreamingService youtubeStreamingService,
                         VlcService vlcService,
                         DownloadedAudioService downloadedAudioService,
                         SettingsService settingsService)
    {
        this.youtubeStreamingService = youtubeStreamingService;
        this.vlcService = vlcService;
        this.downloadedAudioService = downloadedAudioService;
        this.settingsService = settingsService;

        vlcService.mediaPlayer.EndReached += OnAudioEnd;
        vlcService.mediaPlayer.PositionChanged += OnAudioPositionChanged;
    }

    public async Task Start(RuntimePlaylist playlist, PlayerCallbacks callbacks)
    {
        if (started)
            Stop();
        started = true;
        this.playlist = playlist;
        this.callbacks = callbacks;

        await TryChangeAudio();
    }

    public void Stop()
    {
        started = false;
        vlcService.mediaPlayer.Stop();
    }

    public void TogglePause() => vlcService.mediaPlayer.Pause();

    public async Task Next()
    {
        playlist?.Next(settingsService.Setting.Loop);

        await TryChangeAudio();
    }

    public async Task Previous()
    {
        var inFirst20Sec = vlcService.mediaPlayer.Time <= 20 * 1000;
        if (inFirst20Sec)
            playlist?.Previous();

        var restart = !inFirst20Sec || !await TryChangeAudio(true);

        if (restart)
        {
            vlcService.mediaPlayer.Stop();
            vlcService.mediaPlayer.Play();
        }
    }

    public async Task<int> SetVolume(int volume)
    {
        settingsService.Setting.Volume = Math.Clamp(settingsService.Setting.Volume + volume, 0, 100);
        await settingsService.Save();
        vlcService.mediaPlayer.Volume = settingsService.Setting.Volume;

        return settingsService.Setting.Volume;
    }

    public async Task ToggleLooping()
    {
        settingsService.Setting.Loop = !settingsService.Setting.Loop;
        await settingsService.Save();
    }

    async Task<bool> TryChangeAudio(bool canRestart = false)
    {
        if (currentAudioInfo == playlist?.Current)
            return !canRestart;

        currentAudioInfo = playlist?.Current;

        if (currentAudioInfo is null)
            return false;

        var audioStream = downloadedAudioService.TryGetAudio(currentAudioInfo.Id);

        if (audioStream is null)
        {
            var downloadComplete = new TaskCompletionSource();
            callbacks?.OnDownload?.Invoke(downloadComplete.Task).Forget();

            audioStream = await youtubeStreamingService.GetAudioStream(currentAudioInfo.Id);

            downloadComplete.SetResult();
        }

        if (audioStream is null)
            throw new NullReferenceException("Audio stream is null");

        await vlcService.Play(audioStream);

        callbacks?.OnAudioChanged?.Invoke(currentAudioInfo, playlist!.CurrentIndex).Forget();

        return true;
    }

    void OnAudioPositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e) => callbacks?.OnPositionChanged?.Invoke(e.Position);
    void OnAudioEnd(object? sender, EventArgs e) => OnAudioEndAsync().Forget();

    async Task OnAudioEndAsync()
    {
        playlist?.Next(settingsService.Setting.Loop);
        await TryChangeAudio();
    }

    public class PlayerCallbacks
    {
        public Func<AudioInfo, int, Task>? OnAudioChanged { get; init; }
        public Func<Task, Task>? OnDownload { get; init; }
        public Action<float>? OnPositionChanged { get; init; }
    }

    public void Dispose() => Stop();
}