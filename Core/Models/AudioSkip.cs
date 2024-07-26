namespace Core.Models;

public readonly record struct AudioSkip
{
    /// <summary>
    /// In millisecond
    /// </summary>
    public required long Start { get; init; }
    
    /// <summary>
    /// In millisecond
    /// </summary>
    public required long End { get; init; }
}