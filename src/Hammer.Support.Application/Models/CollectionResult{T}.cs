namespace Hammer.Support.Application.Models;

/// <summary>
/// Collection result that also carries the collected items.
/// </summary>
/// <typeparam name="T">The type of collected items.</typeparam>
public sealed record CollectionResult<T> : CollectionResult
{
    /// <summary>Gets the collected items.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];
}
