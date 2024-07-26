using Google.Apis.YouTube.v3;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Core.Services;

public class YoutubeStreamingService
{
    readonly YoutubeClient youtubeStream;
    readonly YouTubeService youtubeApi;

    public YoutubeStreamingService(YoutubeClient youtubeStream, YouTubeService youtubeApi)
    {
        this.youtubeStream = youtubeStream;
        this.youtubeApi = youtubeApi;
    }
    
    public async Task<Stream> GetAudioStream(string id)
    {
        var url = $"https://youtube.com/watch?v={id}";
        var streamManifest = await youtubeStream.Videos.Streams.GetManifestAsync(url);

        var streamInfo = streamManifest
            .GetAudioOnlyStreams()
            .Where(s => s.Container == Container.Mp4)
            .GetWithHighestBitrate();

        
        var stream = await youtubeStream.Videos.Streams.GetAsync(streamInfo);
        
        return stream;
    }

    public async Task<IEnumerable<(string Title, string Id)>> GetMyPlaylists()
    {
        var playlistRequest = youtubeApi.Playlists.List("snippet");
        playlistRequest.Mine = true;
        playlistRequest.MaxResults = 50;

        var playlistResponse = await playlistRequest.ExecuteAsync();

        return playlistResponse.Items.Select(p => (p.Snippet.Title, p.Id));
    }
    public async Task<IEnumerable<(string Title, string Id)>> GetPlaylistAudios(string playlistId)
    {
        var audiosRequest = youtubeApi.PlaylistItems.List("snippet");
        audiosRequest.PlaylistId = playlistId;
        audiosRequest.MaxResults = 50;
        
        var audiosResponse = await audiosRequest.ExecuteAsync();
        
        return audiosResponse.Items.Select(p => (p.Snippet.Title, p.Snippet.ResourceId.VideoId));
    }
}
