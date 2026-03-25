using System.Diagnostics.CodeAnalysis;
using Hammer.Support.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Onbid;

/// <summary>
///     Background service that triggers KAMCO auction collection daily at a configurable hour (KST).
///     For multi-pod deployments, ensure only one replica runs this job
///     or introduce a distributed lock (e.g. Redis) to prevent duplicate execution.
/// </summary>
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Daily batch job, logging overhead negligible")]
public sealed class KamcoCollectionJob : BackgroundService
{
    private static readonly TimeZoneInfo _kst = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
    private readonly ILogger<KamcoCollectionJob> _logger;
    private readonly OnbidOptions _options;

    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KamcoCollectionJob" /> class.
    /// </summary>
    /// <param name="scopeFactory">Service scope factory for resolving scoped dependencies.</param>
    /// <param name="options">Onbid configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public KamcoCollectionJob(
        IServiceScopeFactory scopeFactory,
        IOptions<OnbidOptions> options,
        ILogger<KamcoCollectionJob> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    ///     Calculates the delay from now until the next run at <paramref name="targetHour" /> KST.
    /// </summary>
    /// <param name="targetHour">Hour of day (0-23) in KST to schedule the next run.</param>
    /// <returns>Time remaining until the next scheduled run.</returns>
    internal static TimeSpan CalculateDelayUntilNextRun(int targetHour)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset now_kst = TimeZoneInfo.ConvertTime(now, _kst);

        DateTime targetDate = now_kst.Date;

        if (now_kst.TimeOfDay >= new TimeSpan(targetHour, 0, 0))
            targetDate = targetDate.AddDays(1);

        DateTime targetLocal = targetDate.Add(new TimeSpan(targetHour, 0, 0));
        TimeSpan targetOffset = _kst.GetUtcOffset(targetLocal);
        DateTimeOffset target_kst = new(targetLocal, targetOffset);

        return target_kst - now;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay = CalculateDelayUntilNextRun(_options.CollectionHour);
            _logger.LogInformation("Next KAMCO collection run in {Delay}", delay);

            await Task.Delay(delay, stoppingToken);

            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            ICollectKamcoAuctionsUseCase useCase = scope.ServiceProvider.GetRequiredService<ICollectKamcoAuctionsUseCase>();
            await useCase.ExecuteAsync(stoppingToken);
        }
    }
}
