using Hammer.Support.Application.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Client for fetching KAMCO public auction items from the Onbid API.
/// </summary>
public interface IKamcoApiClient
{
    /// <summary>
    /// Fetches a single page of KAMCO auction items.
    /// </summary>
    /// <param name="pageNo">The 1-based page number.</param>
    /// <param name="numOfRows">Number of rows per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The fetched page result containing items and total count.</returns>
    public Task<KamcoPageResult> FetchPageAsync(int pageNo, int numOfRows, CancellationToken cancellationToken = default);
}
