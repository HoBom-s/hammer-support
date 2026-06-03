using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Collects KAMCO auction items from Onbid and publishes them to Kafka.
/// </summary>
public interface ICollectKamcoAuctionsUseCase
{
    /// <summary>
    /// Executes the collection. Returns immediately with <see cref="CollectionResult.Skipped"/> = true
    /// if another run is already in progress.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection result including collected items.</returns>
    public Task<CollectionResult<KamcoAuctionItem>> ExecuteAsync(CancellationToken cancellationToken = default);
}
