using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Onbid;

/// <summary>
///     Fetches all KAMCO auction pages from Onbid and publishes each item to Kafka.
///     A process-level lock prevents concurrent execution from both scheduled and manual triggers.
/// </summary>
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Batch job, logging overhead negligible")]
public sealed class CollectKamcoAuctionsUseCase : ICollectKamcoAuctionsUseCase
{
    private const string Topic = "onbid-kamco-auction";

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static readonly SemaphoreSlim _runLock = new(1, 1);

    private readonly IKamcoApiClient _client;
    private readonly ILogger<CollectKamcoAuctionsUseCase> _logger;
    private readonly OnbidOptions _options;
    private readonly IEventPublisher _publisher;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CollectKamcoAuctionsUseCase" /> class.
    /// </summary>
    /// <param name="client">Onbid API client.</param>
    /// <param name="publisher">Kafka event publisher.</param>
    /// <param name="options">Onbid configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public CollectKamcoAuctionsUseCase(
        IKamcoApiClient client,
        IEventPublisher publisher,
        IOptions<OnbidOptions> options,
        ILogger<CollectKamcoAuctionsUseCase> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _client = client;
        _publisher = publisher;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CollectionResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!await _runLock.WaitAsync(0, cancellationToken))
        {
            _logger.LogWarning("KAMCO collection already in progress, skipping");
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
        _logger.LogInformation("Starting KAMCO auction collection");

        var sw = Stopwatch.StartNew();
        var pageNo = 1;
        var totalProcessed = 0;
        var totalFailed = 0;
        var totalCount = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                KamcoPageResult page = await _client.FetchPageAsync(pageNo, _options.PageSize, cancellationToken);

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

        return new CollectionResult
        {
            TotalCount = totalCount, Processed = totalProcessed, Failed = totalFailed, ElapsedMs = sw.ElapsedMilliseconds,
        };
    }
}
