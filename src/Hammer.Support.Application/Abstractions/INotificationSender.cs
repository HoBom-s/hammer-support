using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
///     Sends a notification through a specific channel.
/// </summary>
public interface INotificationSender
{
    /// <summary>Gets the channel this sender handles.</summary>
    public NotificationChannel Channel { get; }

    /// <summary>
    ///     Sends a notification to the specified recipient.
    /// </summary>
    /// <param name="recipientToken">Recipient device token or user identifier.</param>
    /// <param name="title">Notification title.</param>
    /// <param name="body">Notification body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    public Task SendAsync(string recipientToken, string title, string body, CancellationToken cancellationToken = default);
}
