using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Client for the Naver News Search API.
/// </summary>
public interface INaverNewsApiClient
{
    /// <summary>
    /// Searches for news articles via the Naver News Search API.
    /// </summary>
    /// <param name="query">The search keyword.</param>
    /// <param name="display">Number of results to return (1-100, default 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of news articles.</returns>
    public Task<IReadOnlyList<NewsArticle>> SearchAsync(
        string query,
        int display = 100,
        CancellationToken cancellationToken = default);
}
