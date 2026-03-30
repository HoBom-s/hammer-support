namespace Hammer.Support.Domain.Models;

/// <summary>
/// Status of a sent notification.
/// </summary>
public enum NotificationStatus
{
    /// <summary>Notification is queued for delivery.</summary>
    Pending,

    /// <summary>Notification was delivered successfully.</summary>
    Sent,

    /// <summary>Notification delivery failed.</summary>
    Failed,
}
