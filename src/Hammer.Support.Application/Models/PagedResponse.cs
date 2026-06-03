namespace Hammer.Support.Application.Models;

/// <summary>
/// Generic paged response wrapper.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
/// <param name="Items">The items on the current page.</param>
/// <param name="Page">The current page number (1-based).</param>
/// <param name="Size">The page size.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
/// <param name="TotalPages">The total number of pages.</param>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int Size,
    int TotalCount,
    int TotalPages);
