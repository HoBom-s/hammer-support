using Hammer.Support.Application.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Client for fetching category codes from the Onbid code information API.
/// </summary>
public interface ICodeInfoApiClient
{
    /// <summary>
    /// Fetches a single page of code information items.
    /// </summary>
    /// <param name="pageNo">The 1-based page number.</param>
    /// <param name="numOfRows">Number of rows per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The fetched page result containing items and total count.</returns>
    public Task<CodeInfoPageResult> FetchPageAsync(int pageNo, int numOfRows, CancellationToken cancellationToken = default);
}
