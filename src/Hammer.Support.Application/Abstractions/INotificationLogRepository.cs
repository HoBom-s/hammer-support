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

    /// <summary>
    /// Updates an existing notification log entry.
    /// </summary>
    /// <param name="log">The notification log to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public Task UpdateAsync(NotificationLog log, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves notification logs for a specific recipient.
    /// When <paramref name="sinceId"/> is null, returns the most recent logs in descending order.
    /// When <paramref name="sinceId"/> is provided, returns logs created after that ID in ascending order.
    /// </summary>
    /// <param name="recipientToken">The recipient token to filter by.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="sinceId">Optional cursor — only return logs created after this log ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of notification logs for the given recipient.</returns>
    public Task<IReadOnlyList<NotificationLog>> GetByRecipientAsync(string recipientToken, int limit, Guid? sinceId, CancellationToken cancellationToken = default);
}
