using System.ComponentModel.DataAnnotations;
using YoutubeExplode.Videos;

namespace EmyAudio.Models;

public record AudioSkip
{
    [Key]
    public required Guid Id { get; init; }
    /// <summary>
    /// In millisecond
    /// </summary>
    public required long Start { get; init; }
    
    /// <summary>
    /// In millisecond
    /// </summary>
    public required long End { get; init; }

    public string AudioInfoId { get; init; }
    public AudioInfo AudioInfo { get; init; } = null!;
}