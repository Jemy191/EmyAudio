using YoutubeExplode.Videos;

namespace Core.Models;

public class DownloadedAudioInfo
{
    public required VideoId Id { get; init; }
    public DateOnly LastPlayed { get; set; } = DateOnly.MinValue;
    public uint TimePlayed { get; set; } = 0;
}