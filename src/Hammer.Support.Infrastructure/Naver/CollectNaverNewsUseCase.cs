using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Naver;

/// <summary>
///     Fetches news articles for configured keywords and saves new ones to the database.
///     A process-level lock prevents concurrent execution from both scheduled and manual triggers.
/// </summary>
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Batch job")]
public sealed class CollectNaverNewsUseCase : ICollectNewsUseCase
{
    private static readonly SemaphoreSlim _runLock = new(1, 1);

    private readonly INaverNewsApiClient _apiClient;
    private readonly INewsArticleRepository _repository;
    private readonly NaverOptions _options;
    private readonly ILogger<CollectNaverNewsUseCase> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CollectNaverNewsUseCase"/> class.
    /// </summary>
    /// <param name="apiClient">Naver News API client.</param>
    /// <param name="repository">News article repository.</param>
    /// <param name="options">Naver configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public CollectNaverNewsUseCase(
        INaverNewsApiClient apiClient,
        INewsArticleRepository repository,
        IOptions<NaverOptions> options,
        ILogger<CollectNaverNewsUseCase> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _apiClient = apiClient;
        _repository = repository;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CollectionResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!await _runLock.WaitAsync(0, cancellationToken))
        {
            _logger.LogWarning("News collection already in progress, skipping");
            return new CollectionResult { Skipped = true };
        }

        try
        {
            return await RunCoreAsync(cancellationToken);
        }
        finally
        {
            _runLock.Release();
        }
    }

    private async Task<CollectionResult> RunCoreAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting news collection for keywords: {Keywords}", string.Join(", ", _options.Keywords));

        var sw = Stopwatch.StartNew();
        var totalFetched = 0;
        var totalSaved = 0;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (var keyword in _options.Keywords)
        {
            try
            {
                IReadOnlyList<NewsArticle> articles = await _apiClient.SearchAsync(keyword, _options.Display, cancellationToken);
                totalFetched += articles.Count;

                List<NewsArticle> newArticles = [];

                foreach (NewsArticle article in articles)
                {
                    if (await _repository.ExistsAsync(article.OriginalLink, cancellationToken))
                        continue;

                    article.Id = Guid.NewGuid();
                    article.Query = keyword;
                    article.CollectedAt = now;
                    newArticles.Add(article);
                }

                if (newArticles.Count > 0)
                {
                    await _repository.AddRangeAsync(newArticles, cancellationToken);
                    totalSaved += newArticles.Count;
                }

                _logger.LogInformation(
                    "Keyword '{Keyword}': fetched {Fetched}, saved {Saved} new articles",
                    keyword,
                    articles.Count,
                    newArticles.Count);
            }
#pragma warning disable CA1031
            catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
            {
                _logger.LogError(ex, "Failed to collect news for keyword '{Keyword}'", keyword);
            }
        }

        sw.Stop();

        _logger.LogInformation(
            "News collection completed: {Saved}/{Fetched} new articles in {ElapsedMs}ms",
            totalSaved,
            totalFetched,
            sw.ElapsedMilliseconds);

        return new CollectionResult
        {
            TotalCount = totalFetched,
            Processed = totalSaved,
            ElapsedMs = sw.ElapsedMilliseconds,
        };
    }
}
