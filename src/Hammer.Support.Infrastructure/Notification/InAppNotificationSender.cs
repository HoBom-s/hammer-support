using Hammer.Support.Application.Abstractions;
using Hammer.Support.Domain.Models;

namespace Hammer.Support.Infrastructure.Notification;

/// <summary>
/// Saves notifications as in-app records in the database.
/// </summary>
internal sealed class InAppNotificationSender : INotificationSender
{
    /// <inheritdoc />
    public NotificationChannel Channel => NotificationChannel.InApp;

    /// <inheritdoc />
    public Task SendAsync(string recipientToken, string title, string body, CancellationToken cancellationToken = default)
    {
        // In-app notifications are persisted as logs by the orchestrator.
        // No external delivery needed — this is a no-op sender.
        return Task.CompletedTask;
    }
}
