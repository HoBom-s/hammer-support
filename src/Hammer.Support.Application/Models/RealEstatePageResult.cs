using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Models;

/// <summary>
/// Represents a paginated response from the MOLIT real estate trade API.
/// </summary>
public sealed record RealEstatePageResult
{
    /// <summary>Gets the total number of items available.</summary>
    public int TotalCount { get; init; }

    /// <summary>Gets the trade items in this page.</summary>
    public IReadOnlyList<RealEstateTrade> Items { get; init; } = [];
}
