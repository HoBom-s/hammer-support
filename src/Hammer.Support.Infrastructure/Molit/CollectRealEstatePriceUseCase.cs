using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using Hammer.Support.Application;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Molit;

/// <summary>
///     Fetches real estate trade data from the MOLIT API for districts where KAMCO auction items
///     are located, then publishes each trade record to Kafka.
/// </summary>
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Batch job, logging overhead negligible")]
public sealed class CollectRealEstatePriceUseCase : ICollectRealEstatePriceUseCase
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static readonly SemaphoreSlim _runLock = new(1, 1);

    private readonly IRealEstateApiClient _client;
    private readonly ILawdCodeResolver _lawdCodeResolver;
    private readonly ILogger<CollectRealEstatePriceUseCase> _logger;
    private readonly MolitOptions _options;
    private readonly IEventPublisher _publisher;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CollectRealEstatePriceUseCase" /> class.
    /// </summary>
    /// <param name="client">The MOLIT API client.</param>
    /// <param name="publisher">The Kafka event publisher.</param>
    /// <param name="lawdCodeResolver">The district code resolver.</param>
    /// <param name="options">The MOLIT configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public CollectRealEstatePriceUseCase(
        IRealEstateApiClient client,
        IEventPublisher publisher,
        ILawdCodeResolver lawdCodeResolver,
        IOptions<MolitOptions> options,
        ILogger<CollectRealEstatePriceUseCase> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _client = client;
        _publisher = publisher;
        _lawdCodeResolver = lawdCodeResolver;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CollectionResult> ExecuteAsync(
        IReadOnlyList<KamcoAuctionItem> kamcoItems,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kamcoItems);

        if (!await _runLock.WaitAsync(0, cancellationToken))
        {
            _logger.LogWarning("Real estate price collection already in progress, skipping");
            return new CollectionResult { Skipped = true };
        }

        try
        {
            return await RunCoreAsync(kamcoItems, cancellationToken);
        }
        finally
        {
            _runLock.Release();
        }
    }

    /// <summary>
    ///     Generates a list of deal year-month strings (yyyyMM) for the most recent <paramref name="months" /> months.
    /// </summary>
    /// <param name="months">The number of months to generate, counting back from the current month.</param>
    /// <returns>A list of year-month strings in descending chronological order.</returns>
    internal static List<string> GenerateDealYmds(int months)
    {
        List<string> result = [];
        DateTime now = DateTime.UtcNow;

        for (var i = 0; i < months; i++)
        {
            DateTime target = now.AddMonths(-i);
            result.Add(target.ToString("yyyyMM", CultureInfo.InvariantCulture));
        }

        return result;
    }

    private async Task<CollectionResult> RunCoreAsync(
        IReadOnlyList<KamcoAuctionItem> kamcoItems,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting real estate price collection from {ItemCount} KAMCO items", kamcoItems.Count);

        var sw = Stopwatch.StartNew();

        // 1. Extract unique (LAWD_CD, PropertyType) pairs
        HashSet<(string LawdCd, PropertyType Type)> queries = [];
        var unmappedCount = 0;

        foreach (KamcoAuctionItem item in kamcoItems)
        {
            var lawdCd = _lawdCodeResolver.Resolve(item.LdnmAdrs);
            PropertyType type = PropertyTypeClassifier.Classify(item.CtgrFullNm);

            if (lawdCd is null || type == PropertyType.Unknown)
            {
                unmappedCount++;
                continue;
            }

            queries.Add((lawdCd, type));
        }

        _logger.LogInformation(
            "Resolved {QueryCount} unique (district, type) pairs from KAMCO items ({Unmapped} unmapped)",
            queries.Count,
            unmappedCount);

        if (queries.Count == 0)
        {
            sw.Stop();
            return new CollectionResult { ElapsedMs = sw.ElapsedMilliseconds };
        }

        // 2. Generate deal year-months to query
        List<string> dealYmds = GenerateDealYmds(_options.MonthsToCollect);

        // 3. Fetch and publish
        var totalProcessed = 0;
        var totalFailed = 0;

        foreach ((string LawdCd, PropertyType Type) query in queries)
        {
            foreach (var dealYmd in dealYmds)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                (int Processed, int Failed) counts = await FetchAndPublishAsync(
                    query.Type,
                    query.LawdCd,
                    dealYmd,
                    cancellationToken);

                totalProcessed += counts.Processed;
                totalFailed += counts.Failed;

                await Task.Delay(200, cancellationToken);
            }
        }

        sw.Stop();

        _logger.LogInformation(
            "Real estate price collection completed: {Processed} items in {ElapsedMs}ms ({Queries} queries x {Months} months, {Failed} failed)",
            totalProcessed,
            sw.ElapsedMilliseconds,
            queries.Count,
            dealYmds.Count,
            totalFailed);

        return new CollectionResult
        {
            TotalCount = totalProcessed, Processed = totalProcessed, Failed = totalFailed, ElapsedMs = sw.ElapsedMilliseconds,
        };
    }

    private async Task<(int Processed, int Failed)> FetchAndPublishAsync(
        PropertyType propertyType,
        string lawdCd,
        string dealYmd,
        CancellationToken cancellationToken)
    {
        var pageNo = 1;
        var processed = 0;
        var failed = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                RealEstatePageResult page = await _client.FetchPageAsync(
                    propertyType,
                    lawdCd,
                    dealYmd,
                    pageNo,
                    _options.PageSize,
                    cancellationToken);

                if (page.Items.Count == 0)
                    break;

                foreach (RealEstateTrade trade in page.Items)
                {
                    var key = $"{lawdCd}-{propertyType}";
                    var value = JsonSerializer.Serialize(trade, _jsonOptions);

                    await _publisher.PublishAsync(KafkaTopics.RealEstatePrice, key, value, cancellationToken);
                    processed++;
                }

                var failedCount = await _publisher.FlushAsync(cancellationToken);
                failed += failedCount;

                if (processed >= page.TotalCount || page.Items.Count < _options.PageSize)
                    break;

                pageNo++;
                await Task.Delay(200, cancellationToken);
            }
        }
#pragma warning disable CA1031
        catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
        {
            _logger.LogError(
                ex,
                "Failed to fetch {PropertyType} trades for LAWD_CD={LawdCd} DEAL_YMD={DealYmd} at page {PageNo}",
                propertyType,
                lawdCd,
                dealYmd,
                pageNo);
        }

        return (processed, failed);
    }
}
