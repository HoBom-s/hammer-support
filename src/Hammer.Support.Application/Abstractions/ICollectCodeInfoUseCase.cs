using Hammer.Support.Application.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Collects code information from Onbid and publishes them to Kafka.
/// </summary>
public interface ICollectCodeInfoUseCase
{
    /// <summary>
    /// Executes the collection. Returns immediately with <see cref="CollectionResult.Skipped"/> = true
    /// if another run is already in progress.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection result.</returns>
    public Task<CollectionResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
