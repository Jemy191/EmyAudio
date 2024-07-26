using YoutubeExplode.Videos;

namespace Core.Models;

public class AudioInfo
{
    public required VideoId Id { get; init; }
    public required string Name { get; init; }
    public required AudioType Type { get; set; }
    public required float Length { get; init; }
    public readonly List<AudioSkip> Skip = [];
    public required int Score { get; set; }
    public readonly List<string> Tags = [];
    public VideoId? Next { get; set; }
    public VideoId? Previous { get; set; }
}