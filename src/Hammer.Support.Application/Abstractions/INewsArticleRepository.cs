using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Repository for news articles collected from external APIs.
/// </summary>
public interface INewsArticleRepository
{
    /// <summary>
    /// Returns the most recent news articles.
    /// </summary>
    /// <param name="limit">Maximum number of articles to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of the latest news articles.</returns>
    public Task<IReadOnlyList<NewsArticle>> GetLatestAsync(int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single news article by its identifier.
    /// </summary>
    /// <param name="id">The article identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The article, or <c>null</c> if not found.</returns>
    public Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of news articles ordered by publication date (newest first).
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="size">The page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The page items and the total article count.</returns>
    public Task<(IReadOnlyList<NewsArticle> Items, int TotalCount)> GetPagedAsync(int page, int size, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of news articles whose title contains the given keyword, newest first.
    /// </summary>
    /// <param name="keyword">The title substring to search for.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="size">The page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching page items and the total match count.</returns>
    public Task<(IReadOnlyList<NewsArticle> Items, int TotalCount)> SearchByTitleAsync(string keyword, int page, int size, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a batch of news articles.
    /// </summary>
    /// <param name="articles">The articles to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddRangeAsync(IEnumerable<NewsArticle> articles, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an article with the given original link already exists.
    /// </summary>
    /// <param name="originalLink">The original article URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the article already exists.</returns>
    public Task<bool> ExistsAsync(string originalLink, CancellationToken cancellationToken = default);
}
