using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Models;

/// <summary>
/// Represents a paginated response from the code information API.
/// </summary>
public sealed record CodeInfoPageResult
{
    /// <summary>
    /// Gets the total number of items available.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the code items in this page.
    /// </summary>
    public IReadOnlyList<OnbidCodeItem> Items { get; init; } = [];
}
