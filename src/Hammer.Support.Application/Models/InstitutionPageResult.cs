using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Models;

/// <summary>
/// Represents a paginated response from the institution auction API.
/// </summary>
public sealed record InstitutionPageResult
{
    /// <summary>
    /// Gets the total number of items available.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the auction items in this page.
    /// </summary>
    public IReadOnlyList<InstitutionAuctionItem> Items { get; init; } = [];
}
