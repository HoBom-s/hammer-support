using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Collects real estate trade prices from the MOLIT API based on KAMCO auction item locations
/// and publishes them to Kafka.
/// </summary>
public interface ICollectRealEstatePriceUseCase
{
    /// <summary>
    /// Extracts unique (district, property type) pairs from the given KAMCO items,
    /// fetches recent trade data from the MOLIT API, and publishes to Kafka.
    /// </summary>
    /// <param name="kamcoItems">KAMCO auction items to derive query parameters from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection result.</returns>
    public Task<CollectionResult> ExecuteAsync(
        IReadOnlyList<KamcoAuctionItem> kamcoItems,
        CancellationToken cancellationToken = default);
}
