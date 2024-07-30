using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YoutubeExplode.Videos;

namespace Core.Models;

public class AudioInfo
{
    [Key]
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required AudioType Type { get; set; }
    public required TimeSpan Duration { get; init; }
    public List<AudioSkip> Skips { get; init; } = [];
    public int Score { get; set; }
    public DateOnly LastPlayed { get; set; } = DateOnly.MinValue;
    public uint TimePlayed { get; set; } = 0;
    public HashSet<Tag> Tags { get; init; } = [];
    public string? Next { get; set; }
    public string? Previous { get; set; }
}