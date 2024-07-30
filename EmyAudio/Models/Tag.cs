using System.ComponentModel.DataAnnotations;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace EmyAudio.Models;

public class Tag
{
    [Key]
    public required string Name { get; set; }
    public required bool Downloaded { get; set; }
    public required TagType Type { get; set; }
    public int Score { get; set; }
    public string? Parent { get; set; }
    public string? Next { get; set; }
    public string? Previous { get; set; }
    public bool Hidden { get; set; }

    public required string? OriginalPlaylistId { get; init; }

    public HashSet<AudioInfo> AudioInfos { get; init; } = [];
}