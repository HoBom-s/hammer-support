using Hammer.Support.Application.Abstractions;
using Hammer.Support.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Support.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="INewsArticleRepository"/>.
/// </summary>
internal sealed class NewsArticleRepository : INewsArticleRepository
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsArticleRepository"/> class.
    /// </summary>
    /// <param name="db">The application database context.</param>
    public NewsArticleRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NewsArticle>> GetLatestAsync(int limit, CancellationToken cancellationToken = default)
    {
        return await _db.NewsArticles
            .AsNoTracking()
            .OrderByDescending(a => a.PubDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.NewsArticles
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<NewsArticle> Items, int TotalCount)> GetPagedAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        IQueryable<NewsArticle> query = _db.NewsArticles
            .AsNoTracking()
            .OrderByDescending(a => a.PubDate);

        var totalCount = await query.CountAsync(cancellationToken);

        List<NewsArticle> items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<NewsArticle> Items, int TotalCount)> SearchByTitleAsync(string keyword, int page, int size, CancellationToken cancellationToken = default)
    {
        IQueryable<NewsArticle> query = _db.NewsArticles
            .AsNoTracking()
            .Where(a => a.Title.Contains(keyword))
            .OrderByDescending(a => a.PubDate);

        var totalCount = await query.CountAsync(cancellationToken);

        List<NewsArticle> items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<NewsArticle> articles, CancellationToken cancellationToken = default)
    {
        _db.NewsArticles.AddRange(articles);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string originalLink, CancellationToken cancellationToken = default)
    {
        return await _db.NewsArticles
            .AnyAsync(a => a.OriginalLink == originalLink, cancellationToken);
    }
}
