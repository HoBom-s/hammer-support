using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Repository for notification logs.
/// </summary>
public interface INotificationLogRepository
{
    /// <summary>
    /// Saves a notification log entry.
    /// </summary>
    /// <param name="log">The notification log to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public Task SaveAsync(NotificationLog log, CancellationToken cancellationToken = default);
}
