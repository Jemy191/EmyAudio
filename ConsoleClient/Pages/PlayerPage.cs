using Core;
using Core.Models;
using Core.Services;
using LibVLCSharp.Shared;
using Spectre.Console;

namespace ConsoleClient.Pages;

public class PlayerPage : IPage
{
    public string Name => "Player";
    readonly YoutubeStreamingService youtube;
    readonly VlcService vlcService;
    readonly PageNavigationService pageNavigationService;
    readonly SettingsService settingsService;
    readonly DownloadedAudioService downloadedAudioService;

    RuntimePlaylist? playlistInfo;
    AudioInfo? currentAudioInfo;

    ProgressTask? audioProgress;
    ProgressTask? volumeProgress;
    readonly List<ProgressTask> allProgressTasks = [];
    ProgressContext? audioProgressContext;
    Task? progressTask;

    public PlayerPage(YoutubeStreamingService youtube,
                      VlcService vlcService,
                      PageNavigationService pageNavigationService,
                      SettingsService settingsService,
                      DownloadedAudioService downloadedAudioService)
    {
        this.youtube = youtube;
        this.vlcService = vlcService;
        this.pageNavigationService = pageNavigationService;
        this.settingsService = settingsService;
        this.downloadedAudioService = downloadedAudioService;

        vlcService.mediaPlayer.EndReached += OnAudioEnd;
        vlcService.mediaPlayer.PositionChanged += OnAudioPositionChanged;
    }

    public async Task Show()
    {
        playlistInfo = await pageNavigationService.Open<TagChoosingPage, RuntimePlaylist>();

        await TryChangeAudio();

        if (await Control())
            return;
    }

    async Task<bool> TryChangeAudio(bool canRestart = false)
    {
        await RefreshPlayerInfo(false);

        if (currentAudioInfo == playlistInfo?.Current)
            return !canRestart;

        currentAudioInfo = playlistInfo?.Current;

        if (currentAudioInfo is null)
            return false;

        var audioStream = downloadedAudioService.TryGetAudio(currentAudioInfo.Id);

        if (audioStream is null)
            await AnsiConsole.Status()
                .StartAsync("Loading audio stream...", async _ =>
                {
                    audioStream = await youtube.GetAudioStream(currentAudioInfo.Id);
                });

        if (audioStream is null)
            throw new NullReferenceException("Audio stream is null");
        
        await vlcService.Play(audioStream);

        return true;
    }

    async Task RefreshPlayerInfo(bool drawAudioProgress = true)
    {
        // This need to be here or the progress bar will not correctly clear.
        foreach (var progressTask in allProgressTasks)
        {
            progressTask.StopTask();
        }
        allProgressTasks.Clear();
        if (progressTask != null)
            await progressTask;

        pageNavigationService.Refresh();

        AnsiConsole.MarkupLineInterpolated($"""
                                            Now playing: {currentAudioInfo?.Name}
                                            Control:
                                            Space -> Toggle pause, M -> Menu, T -> Tag choosing, Escape -> Exit
                                            Left/Right arrow -> Change audio Up/Down arrow -> Volume
                                            Index: {playlistInfo?.CurrentIndex.ToString() ?? "null"}
                                            Looping: {settingsService.Setting.Loop}
                                            """);

        if (drawAudioProgress)
        {
            progressTask = AnsiConsole.Progress()
                .AutoClear(true)
                .StartAsync(async context =>
                {
                    audioProgressContext = context;

                    volumeProgress = context.AddTask("Volume:")
                        .Value(settingsService.Setting.Volume)
                        .MaxValue(100);

                    audioProgress = context.AddTask("Time:")
                        .MaxValue(1);

                    allProgressTasks.Add(volumeProgress);
                    allProgressTasks.Add(audioProgress);

                    while (!audioProgress.IsFinished)
                    {
                        await Task.Delay(100);
                    }
                });
        }
    }

    /// <returns>Is exit requested</returns>
    async Task<bool> Control()
    {
        var dirty = true;
        while (true)
        {
            if (dirty)
                await RefreshPlayerInfo();

            dirty = false;

            if (playlistInfo is null)
                return true;
            
            var key = Console.ReadKey().Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    await ChangeVolume(5);
                    break;
                case ConsoleKey.DownArrow:
                    await ChangeVolume(-5);
                    break;
                case ConsoleKey.RightArrow:
                    playlistInfo.Next(settingsService.Setting.Loop);

                    await TryChangeAudio();
                    dirty = true;
                    break;
                case ConsoleKey.LeftArrow:
                    var inFirst20Sec = vlcService.mediaPlayer.Time <= 20 * 1000;
                    if (inFirst20Sec)
                        playlistInfo.Previous();

                    var restart = !inFirst20Sec || !await TryChangeAudio(true);

                    if (restart)
                    {
                        vlcService.mediaPlayer.Stop();
                        vlcService.mediaPlayer.Play();
                    }
                    dirty = true;
                    break;

                case ConsoleKey.L:
                    settingsService.Setting.Loop = !settingsService.Setting.Loop;
                    await settingsService.Save();
                    dirty = true;
                    break;

                case ConsoleKey.Spacebar:
                    vlcService.mediaPlayer.Pause();
                    break;
                case ConsoleKey.Escape:
                    AnsiConsole.Clear();
                    AnsiConsole.MarkupLine("Exiting");
                    return true;
                case ConsoleKey.T:
                    vlcService.mediaPlayer.Stop();
                    await RefreshPlayerInfo(false);
                    playlistInfo = await pageNavigationService.Open<TagChoosingPage, RuntimePlaylist>();
                    return false;
                case ConsoleKey.M:
                    vlcService.mediaPlayer.Stop();
                    await RefreshPlayerInfo(false);
                    await pageNavigationService.Open<MenuPage>();
                    return true;
            }
        }
    }
    async Task ChangeVolume(int delta)
    {
        settingsService.Setting.Volume = Math.Clamp(settingsService.Setting.Volume + delta, 0, 100);
        await settingsService.Save();
        vlcService.mediaPlayer.Volume = settingsService.Setting.Volume;
        volumeProgress?.Value(settingsService.Setting.Volume);
    }

    void OnAudioPositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
    {
        if (audioProgress is null)
            return;
        audioProgress.Value = e.Position;


        audioProgressContext?.Refresh();
    }

    async void OnAudioEnd(object? sender, EventArgs e)
    {
        try
        {
            playlistInfo?.Next(settingsService.Setting.Loop);
            await TryChangeAudio();
            await RefreshPlayerInfo();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            await Task.Delay(TimeSpan.FromSeconds(30));
            throw;
        }
    }
}