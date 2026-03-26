using Hammer.Support.Application.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Client for fetching institution public sale announcements from the Onbid API.
/// </summary>
public interface IInstitutionApiClient
{
    /// <summary>
    /// Fetches a single page of institution auction announcements.
    /// </summary>
    /// <param name="pageNo">The 1-based page number.</param>
    /// <param name="numOfRows">Number of rows per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The fetched page result containing items and total count.</returns>
    public Task<InstitutionPageResult> FetchPageAsync(int pageNo, int numOfRows, CancellationToken cancellationToken = default);
}
