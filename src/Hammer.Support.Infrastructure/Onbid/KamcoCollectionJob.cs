using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Onbid;

/// <summary>
///     Background service that collects KAMCO auction items daily at 9 AM KST.
///     For multi-pod deployments, ensure only one replica runs this job
///     or introduce a distributed lock (e.g. Redis) to prevent duplicate execution.
/// </summary>
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Daily batch job, logging overhead negligible")]
public sealed class KamcoCollectionJob : BackgroundService
{
    private const string Topic = "onbid-kamco-auction";

    private static readonly TimeZoneInfo _kst = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static readonly SemaphoreSlim _runLock = new(1, 1);

    private readonly ILogger<KamcoCollectionJob> _logger;
    private readonly OnbidOptions _options;
    private readonly IEventPublisher _publisher;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KamcoCollectionJob" /> class.
    /// </summary>
    /// <param name="scopeFactory">Service scope factory for resolving scoped dependencies.</param>
    /// <param name="publisher">Kafka event publisher.</param>
    /// <param name="options">Onbid configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public KamcoCollectionJob(
        IServiceScopeFactory scopeFactory,
        IEventPublisher publisher,
        IOptions<OnbidOptions> options,
        ILogger<KamcoCollectionJob> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _options = options.Value;
        _logger = logger;
    }

    internal static TimeSpan CalculateDelayUntilNextRun(int targetHour)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset nowKst = TimeZoneInfo.ConvertTime(now, _kst);

        DateTime targetDate = nowKst.Date;
        if (nowKst.TimeOfDay >= new TimeSpan(targetHour, 0, 0))
            targetDate = targetDate.AddDays(1);

        DateTime targetLocal = targetDate.Add(new TimeSpan(targetHour, 0, 0));
        TimeSpan targetOffset = _kst.GetUtcOffset(targetLocal);
        DateTimeOffset targetKst = new(targetLocal, targetOffset);

        return targetKst - now;
    }

    /// <summary>
    ///     Fetches all KAMCO auction pages and publishes each item to Kafka.
    ///     Acquires a process-level lock to prevent concurrent execution.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal async Task RunCollectionAsync(CancellationToken cancellationToken)
    {
        if (!await _runLock.WaitAsync(0, cancellationToken))
        {
            _logger.LogWarning("KAMCO collection already in progress, skipping");
            return;
        }

        try
        {
            await RunCollectionCoreAsync(cancellationToken);
        }
        finally
        {
            _runLock.Release();
        }
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay = CalculateDelayUntilNextRun(_options.CollectionHour);
            _logger.LogInformation("Next KAMCO collection run in {Delay}", delay);

            await Task.Delay(delay, stoppingToken);
            await RunCollectionAsync(stoppingToken);
        }
    }

    private async Task RunCollectionCoreAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting KAMCO auction collection");

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        IKamcoApiClient client = scope.ServiceProvider.GetRequiredService<IKamcoApiClient>();

        var sw = Stopwatch.StartNew();
        var pageNo = 1;
        var totalProcessed = 0;
        var totalFailed = 0;
        var totalCount = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                KamcoPageResult page = await client.FetchPageAsync(pageNo, _options.PageSize, cancellationToken);

                if (pageNo == 1)
                    totalCount = page.TotalCount;

                if (page.Items.Count == 0)
                    break;

                foreach (KamcoAuctionItem item in page.Items)
                {
                    var key = $"{item.PlnmNo}-{item.PbctNo}-{item.CltrNo}";
                    var value = JsonSerializer.Serialize(item, _jsonOptions);

                    await _publisher.PublishAsync(Topic, key, value, cancellationToken);
                    totalProcessed++;
                }

                var failedCount = await _publisher.FlushAsync(cancellationToken);
                totalFailed += failedCount;

                if (failedCount > 0)
                {
                    _logger.LogError(
                        "{FailedCount} items failed delivery on page {PageNo}",
                        failedCount,
                        pageNo);
                }

                _logger.LogInformation(
                    "Published page {PageNo} ({Count} items, {Total}/{TotalCount} total)",
                    pageNo,
                    page.Items.Count,
                    totalProcessed,
                    totalCount);

                if (totalProcessed >= totalCount)
                    break;

                pageNo++;
            }
        }
#pragma warning disable CA1031
        catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
        {
            _logger.LogError(
                ex,
                "KAMCO collection failed at page {PageNo} after processing {Processed} items",
                pageNo,
                totalProcessed);
        }

        sw.Stop();
        _logger.LogInformation(
            "KAMCO collection completed: {Delivered}/{TotalCount} items in {ElapsedMs}ms ({Pages} pages, {Failed} failed)",
            totalProcessed - totalFailed,
            totalCount,
            sw.ElapsedMilliseconds,
            pageNo,
            totalFailed);
    }
}
