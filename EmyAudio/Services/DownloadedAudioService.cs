using YoutubeExplode.Videos;

namespace EmyAudio.Services;

public class DownloadedAudioService
{
    readonly string audioDownloadFolder = Path.Combine("Download", "Audios");
    public Stream? TryGetAudio(VideoId id)
    {
        var path = Path.Combine(audioDownloadFolder, $"{id}.mp4");
        if(!File.Exists(path))
            return null;

        return File.OpenRead(path);
    }
    public async Task SaveAudio(Stream audio, VideoId id)
    {
        Directory.CreateDirectory(audioDownloadFolder);

        var path = Path.Combine(audioDownloadFolder, $"{id}.mp4");

        if (Path.Exists(path))
            return;
        
        await using var fileStream = File.OpenWrite(path);
        await audio.CopyToAsync(fileStream);
    }
}