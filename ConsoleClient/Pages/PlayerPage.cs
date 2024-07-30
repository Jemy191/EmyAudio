using EmyAudio;
using EmyAudio.Models;
using EmyAudio.Services;
using Spectre.Console;

namespace ConsoleClient.Pages;

public class PlayerPage : IPage
{
    public string Name => "Player";
    readonly PageNavigationService pageNavigationService;
    readonly SettingsService settingsService;
    readonly PlayerService playerService;

    ProgressTask? audioProgress;
    ProgressTask? volumeProgress;
    readonly List<ProgressTask> allProgressTasks = [];
    ProgressContext? audioProgressContext;
    Task? progressTask;
    AudioInfo? currentAudioInfo;
    int currentIndex;

    public PlayerPage(PageNavigationService pageNavigationService,
                      SettingsService settingsService,
                      PlayerService playerService)
    {
        this.pageNavigationService = pageNavigationService;
        this.settingsService = settingsService;
        this.playerService = playerService;
    }

    public async Task Show()
    {
        var playlistInfo = await pageNavigationService.Open<TagChoosingPage, RuntimePlaylist>();

        await StartPlayer(playlistInfo);

        await Control();
    }
    async Task StartPlayer(RuntimePlaylist playlistInfo)
    {
        var playerCallback = new PlayerService.PlayerCallbacks
        {
            OnDownload = OnDownload,
            OnAudioChanged = OnAudioChanged,
            OnPositionChanged = OnPositionChanged
        };

        await playerService.Start(playlistInfo, playerCallback);
    }

    async Task StopPlayer()
    {
        playerService.Stop();
        await RefreshPlayerInfo(false);
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

        pageNavigationService.RefreshShell();

        AnsiConsole.MarkupLineInterpolated($"""
                                            Playlist: {playerService.CurrentPlaylistName}
                                            Now playing: {currentAudioInfo?.Name}
                                            Control:
                                            Space -> Toggle pause, M -> Menu, T -> Tag choosing, Escape -> Exit
                                            Left/Right arrow -> Change audio Up/Down arrow -> Volume
                                            Index: {currentIndex}
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
    
    async Task ChangeVolume(int newVolume)
    {
        var volume = await playerService.SetVolume(newVolume);
        volumeProgress?.Value(volume);
    }

    void OnPositionChanged(float position)
    {
        if (audioProgress is null)
            return;
        audioProgress.Value = position;
        
        audioProgressContext?.Refresh();
    }

    async Task OnAudioChanged(AudioInfo audioInfo, int currentIndex)
    {
        currentAudioInfo = audioInfo;
        this.currentIndex = currentIndex;
        await RefreshPlayerInfo();
    }

    async Task OnDownload(Task downloadComplete)
    {
        await RefreshPlayerInfo(false);

        await AnsiConsole.Status()
            .StartAsync("Loading audio stream...", async _ =>
            {
                await downloadComplete;
            });
    }

    async Task Control()
    {
        var dirty = true;
        while (true)
        {
            if (dirty)
                await RefreshPlayerInfo();

            dirty = false;

            var key = Console.ReadKey(false).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    await ChangeVolume(5);
                    break;
                case ConsoleKey.DownArrow:
                    await ChangeVolume(-5);
                    break;
                case ConsoleKey.RightArrow:
                    await playerService.Next();
                    break;
                case ConsoleKey.LeftArrow:
                    await playerService.Previous();
                    break;
                case ConsoleKey.L:
                    await playerService.ToggleLooping();
                    dirty = true;
                    break;
                case ConsoleKey.Spacebar:
                    playerService.TogglePause();
                    break;
                case ConsoleKey.Escape:
                    await StopPlayer();
                    AnsiConsole.Clear();
                    AnsiConsole.MarkupLine("Exiting");
                    pageNavigationService.RequestExit();
                    return;
                case ConsoleKey.T:
                    await StopPlayer();
                    var playlistInfo = await pageNavigationService.Open<TagChoosingPage, RuntimePlaylist>();
                    await StartPlayer(playlistInfo);
                    break;
                case ConsoleKey.M:
                    await StopPlayer();
                    return;
            }
        }
    }
}