using Hammer.Support.Application.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Orchestrates notification processing: template lookup, rendering, delivery, and logging.
/// </summary>
public interface INotificationOrchestrator
{
    /// <summary>
    /// Processes a notification request end-to-end.
    /// </summary>
    /// <param name="request">The incoming notification request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous processing operation.</returns>
    public Task ProcessAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
