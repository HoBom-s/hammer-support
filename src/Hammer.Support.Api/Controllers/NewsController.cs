using System.Diagnostics.CodeAnalysis;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Support.Api.Controllers;

/// <summary>
/// Endpoints for querying collected news articles.
/// </summary>
[ApiController]
[Route("api/news")]
[SuppressMessage("Performance", "CA1515:Consider making public types internal", Justification = "MVC requires public controllers for discovery")]
public sealed class NewsController : ControllerBase
{
    private readonly INewsArticleRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsController"/> class.
    /// </summary>
    /// <param name="repository">The news article repository.</param>
    public NewsController(INewsArticleRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Returns a page of news articles ordered by publication date (newest first).
    /// </summary>
    /// <param name="page">Page number (1-based, default 1).</param>
    /// <param name="size">Page size (default 20, max 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 with a paged list of news articles.</returns>
    [HttpGet]
    [ProducesResponseType<PagedResponse<NewsResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPagedAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        size = Math.Clamp(size, 1, 100);

        (IReadOnlyList<NewsArticle> items, var totalCount) = await _repository.GetPagedAsync(page, size, cancellationToken);

        return Ok(ToPagedResponse(items, page, size, totalCount));
    }

    /// <summary>
    /// Returns the most recent news articles.
    /// </summary>
    /// <param name="count">Number of articles to return (default 5, max 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 with a list of the latest news articles.</returns>
    [HttpGet("recent")]
    [ProducesResponseType<IReadOnlyList<NewsResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentAsync(
        [FromQuery] int count = 5,
        CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 100);

        IReadOnlyList<NewsArticle> items = await _repository.GetLatestAsync(count, cancellationToken);

        return Ok(items.Select(NewsResponse.FromEntity).ToList());
    }

    /// <summary>
    /// Returns a single news article by its identifier.
    /// </summary>
    /// <param name="id">The article identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 with the article, or 404 if not found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<NewsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        NewsArticle? article = await _repository.GetByIdAsync(id, cancellationToken);

        return article is null
            ? NotFound()
            : Ok(NewsResponse.FromEntity(article));
    }

    /// <summary>
    /// Searches news articles whose title contains the given keyword (newest first).
    /// </summary>
    /// <param name="keyword">The title substring to search for.</param>
    /// <param name="page">Page number (1-based, default 1).</param>
    /// <param name="size">Page size (default 20, max 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 with the matching paged articles, or 400 if the keyword is missing.</returns>
    [HttpGet("search")]
    [ProducesResponseType<PagedResponse<NewsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest("keyword is required.");

        page = Math.Max(page, 1);
        size = Math.Clamp(size, 1, 100);

        (IReadOnlyList<NewsArticle> items, var totalCount) = await _repository.SearchByTitleAsync(keyword, page, size, cancellationToken);

        return Ok(ToPagedResponse(items, page, size, totalCount));
    }

    private static PagedResponse<NewsResponse> ToPagedResponse(IReadOnlyList<NewsArticle> items, int page, int size, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)size);

        return new PagedResponse<NewsResponse>(
            items.Select(NewsResponse.FromEntity).ToList(),
            page,
            size,
            totalCount,
            totalPages);
    }
}
