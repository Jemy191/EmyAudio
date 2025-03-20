using EmyAudio.Models;

namespace EmyAudio.Services;

public class PlayerService : IDisposable
{
    readonly YoutubeApiService youtubeApiService;
    readonly AudioPlayerService audioPlayerService;
    readonly DownloadedAudioService downloadedAudioService;
    readonly SettingsService settingsService;

    bool started;
    PlayerCallbacks? callbacks;
    RuntimePlaylist? playlist;
    AudioInfo? currentAudioInfo;
    
    public string CurrentPlaylistName => playlist?.Name ?? "Error";

    public PlayerService(YoutubeApiService youtubeApiService,
                         AudioPlayerService audioPlayerService,
                         DownloadedAudioService downloadedAudioService,
                         SettingsService settingsService)
    {
        this.youtubeApiService = youtubeApiService;
        this.audioPlayerService = audioPlayerService;
        this.downloadedAudioService = downloadedAudioService;
        this.settingsService = settingsService;

        audioPlayerService.PlaybackMonitor.OnAudioEnd += OnAudioEnd;
        audioPlayerService.PlaybackMonitor.OnPositionChanged += OnAudioPositionChanged;
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
        audioPlayerService.Stop();
    }

    public void TogglePause() => audioPlayerService.Pause();

    public async Task Next()
    {
        playlist?.Next(settingsService.Setting.Loop);

        await TryChangeAudio();
    }

    public async Task Previous()
    {
        var inFirst20Sec = audioPlayerService.Position <= 20 * 1000;
        if (inFirst20Sec)
            playlist?.Previous();

        await TryChangeAudio(!inFirst20Sec);
    }

    public async Task<int> SetVolume(int volume)
    {
        settingsService.Setting.Volume = Math.Clamp(settingsService.Setting.Volume + volume, 0, 100);
        await settingsService.Save();
        audioPlayerService.Volume = settingsService.Setting.Volume;

        return settingsService.Setting.Volume;
    }

    public async Task ToggleLooping()
    {
        settingsService.Setting.Loop = !settingsService.Setting.Loop;
        audioPlayerService.IsLooping = settingsService.Setting.Loop;
        await settingsService.Save();
    }
    
    async Task TryChangeAudio(bool canRestart = false)
    {
        var restartAudio = currentAudioInfo == playlist?.Current && canRestart;
        var hasOneAudio = playlist?.Count == 1;
        
        if (restartAudio)
        {
            audioPlayerService.Reset();
            return;
        }

        if(hasOneAudio && settingsService.Setting.Loop)
            audioPlayerService.Reset();
        
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

            audioStream = await youtubeApiService.GetAudioStream(currentAudioInfo.Id);

            var streamToSave = new MemoryStream();
            await audioStream.CopyToAsync(streamToSave);
            audioSavingTask = downloadedAudioService.SaveAudio(streamToSave, currentAudioInfo.Id);

            downloadComplete.SetResult();
        }

        if (audioStream is null)
            throw new NullReferenceException("Audio stream is null");

        audioPlayerService.IsLooping = settingsService.Setting.Loop;
        audioPlayerService.Play(audioStream);

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