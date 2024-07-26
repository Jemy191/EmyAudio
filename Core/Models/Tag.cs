using YoutubeExplode.Playlists;

namespace Core.Models;

public class Tag
{
    public required string Name { get; set; }
    public required bool Downloaded { get; set; }
    public required TagType Type { get; set; }
    public required int Score { get; set; }
    public string? Parent { get; set; }
    public string? Next { get; set; }
    public string? Previous { get; set; }

    public required PlaylistId? OriginalPlaylistId { get; init; }

    public Tag()
    {
        
    }
}