namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Publishes domain events to a message broker.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Enqueues a message for delivery to the specified topic.
    /// This method is non-blocking; call <see cref="FlushAsync"/> to ensure delivery.
    /// </summary>
    /// <param name="topic">Target topic name.</param>
    /// <param name="key">Message key for partitioning.</param>
    /// <param name="value">Serialized message payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the enqueue operation.</returns>
    public Task PublishAsync(string topic, string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Flushes all pending messages and returns the number of delivery failures since the last flush.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of messages that failed delivery.</returns>
    public Task<int> FlushAsync(CancellationToken cancellationToken = default);
}
