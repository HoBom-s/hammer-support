using Hammer.Support.Application.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Collects news articles from an external API and saves them to the database.
/// </summary>
public interface ICollectNewsUseCase
{
    /// <summary>
    /// Executes the collection. Returns immediately with <see cref="CollectionResult.Skipped"/> = true
    /// if another run is already in progress.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection result.</returns>
    public Task<CollectionResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
