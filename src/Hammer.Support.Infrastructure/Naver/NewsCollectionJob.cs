using System.Diagnostics.CodeAnalysis;
using Hammer.Support.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Naver;

/// <summary>
///     Background service that periodically collects news articles from the Naver News API.
/// </summary>
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Batch job")]
public sealed class NewsCollectionJob : BackgroundService
{
    private readonly ILogger<NewsCollectionJob> _logger;
    private readonly NaverOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NewsCollectionJob" /> class.
    /// </summary>
    /// <param name="scopeFactory">Service scope factory for resolving scoped dependencies.</param>
    /// <param name="options">Naver configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public NewsCollectionJob(
        IServiceScopeFactory scopeFactory,
        IOptions<NaverOptions> options,
        ILogger<NewsCollectionJob> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var interval = TimeSpan.FromMinutes(_options.CollectionIntervalMinutes);
            _logger.LogInformation("Next news collection run in {Interval}", interval);

            await Task.Delay(interval, stoppingToken);

            await RunCollectionAsync(stoppingToken);
        }
    }

    private async Task RunCollectionAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

        _logger.LogInformation("Starting scheduled news collection");

        ICollectNewsUseCase useCase = scope.ServiceProvider.GetRequiredService<ICollectNewsUseCase>();
        await useCase.ExecuteAsync(stoppingToken);

        _logger.LogInformation("Scheduled news collection completed");
    }
}
