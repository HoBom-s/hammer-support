namespace Hammer.Support.Application.Models;

/// <summary>
/// Result of a KAMCO auction collection run.
/// </summary>
public sealed record CollectionResult
{
    /// <summary>Gets a value indicating whether the run was skipped because another is already in progress.</summary>
    public bool Skipped { get; init; }

    /// <summary>Gets the total item count reported by the API.</summary>
    public int TotalCount { get; init; }

    /// <summary>Gets the number of items successfully processed.</summary>
    public int Processed { get; init; }

    /// <summary>Gets the number of items that failed Kafka delivery.</summary>
    public int Failed { get; init; }

    /// <summary>Gets the elapsed wall-clock time in milliseconds.</summary>
    public long ElapsedMs { get; init; }
}
