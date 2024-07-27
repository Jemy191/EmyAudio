using Core.Models;

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
    
    public string CurrentPlaylistName => playlist?.Name ?? "Error";

    public PlayerService(YoutubeStreamingService youtubeStreamingService,
                         VlcService vlcService,
                         DownloadedAudioService downloadedAudioService,
                         SettingsService settingsService)
    {
        this.youtubeStreamingService = youtubeStreamingService;
        this.vlcService = vlcService;
        this.downloadedAudioService = downloadedAudioService;
        this.settingsService = settingsService;

        vlcService.OnAudioEnd += OnAudioEnd;
        vlcService.OnPositionChanged += OnAudioPositionChanged;
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
        vlcService.mediaPlayer!.Stop();
    }

    public void TogglePause() => vlcService.mediaPlayer!.Pause();

    public async Task Next()
    {
        playlist?.Next(settingsService.Setting.Loop);

        await TryChangeAudio();
    }

    public async Task Previous()
    {
        var inFirst20Sec = vlcService.mediaPlayer!.Time <= 20 * 1000;
        if (inFirst20Sec)
            playlist?.Previous();

        await TryChangeAudio(!inFirst20Sec);
    }

    public async Task<int> SetVolume(int volume)
    {
        settingsService.Setting.Volume = Math.Clamp(settingsService.Setting.Volume + volume, 0, 100);
        await settingsService.Save();
        vlcService.mediaPlayer!.Volume = settingsService.Setting.Volume;

        return settingsService.Setting.Volume;
    }

    public async Task ToggleLooping()
    {
        settingsService.Setting.Loop = !settingsService.Setting.Loop;
        await settingsService.Save();
    }
    
    async Task TryChangeAudio(bool canRestart = false)
    {
        var restartAudio = currentAudioInfo == playlist?.Current && canRestart;
        var hasOneAudio = playlist?.Count == 1 && vlcService.mediaPlayer!.Media is not null;
        
        if (restartAudio)
        {
            vlcService.mediaPlayer!.Stop();
            vlcService.mediaPlayer.Play();
            return;
        }

        if(hasOneAudio && settingsService.Setting.Loop)
            vlcService.Reset();
        
        if(hasOneAudio && !settingsService.Setting.Loop)
            return;
        

        currentAudioInfo = playlist?.Current;

        if (currentAudioInfo is null)
            return;

        var audioStream = downloadedAudioService.TryGetAudio(currentAudioInfo.Id);

        Task? audioSavingTask = null;
        if (audioStream is null)
        {
            var downloadComplete = new TaskCompletionSource();
            callbacks?.OnDownload?.Invoke(downloadComplete.Task).Forget();

            audioStream = await youtubeStreamingService.GetAudioStream(currentAudioInfo.Id);

            var streamToSave = new MemoryStream();
            await audioStream.CopyToAsync(streamToSave);
            audioSavingTask = downloadedAudioService.SaveAudio(streamToSave, currentAudioInfo.Id);

            downloadComplete.SetResult();
        }

        if (audioStream is null)
            throw new NullReferenceException("Audio stream is null");

        await vlcService.Play(audioStream);

        callbacks?.OnAudioChanged?.Invoke(currentAudioInfo, playlist!.CurrentIndex).Forget();

        if (audioSavingTask is not null)
            await audioSavingTask;
    }

    void OnAudioPositionChanged(float position) => callbacks?.OnPositionChanged?.Invoke(position);
    void OnAudioEnd() => OnAudioEndAsync().Forget();

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