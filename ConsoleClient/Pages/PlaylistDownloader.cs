using EmyAudio;
using EmyAudio.Services;
using Spectre.Console;

namespace ConsoleClient.Pages;

public class PlaylistDownloader : IPage
{
    readonly YoutubeApiService youtubeApiService;
    readonly PageNavigationService pageNavigationService;
    readonly DownloadedAudioService downloadedAudioService;
    public string Name => "Playlist downloader";

    public PlaylistDownloader(YoutubeApiService youtubeApiService, PageNavigationService pageNavigationService, DownloadedAudioService downloadedAudioService)
    {
        this.youtubeApiService = youtubeApiService;
        this.pageNavigationService = pageNavigationService;
        this.downloadedAudioService = downloadedAudioService;
    }

    public async Task Show()
    {
        var playlistInfo = await pageNavigationService.Open<TagChoosingPage, RuntimePlaylist>();

        await AnsiConsole.Progress()
            .StartAsync(async context =>
            {
                var task = context.AddTask("Downloading")
                    .MaxValue(playlistInfo.Count);
                
                var tasks = playlistInfo.AudioInfos.Select(audio => Task.Run(async () =>
                    {
                        var audioStream = await youtubeApiService.GetAudioStream(audio.Id);
                        await downloadedAudioService.SaveAudio(audioStream, audio.Id);

                        lock (task)
                        {
                            task.Increment(1);
                        }
                    }))
                    .ToList();

                await Task.WhenAll(tasks);
            });
    }
}