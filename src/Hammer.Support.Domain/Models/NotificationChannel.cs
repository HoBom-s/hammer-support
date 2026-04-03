namespace Hammer.Support.Domain.Models;

/// <summary>
///     Notification delivery channel.
/// </summary>
public enum NotificationChannel
{
    /// <summary>Push notification via Expo Push API.</summary>
    Push,

    /// <summary>In-app notification stored in the database.</summary>
    InApp,

    /// <summary>Both push and in-app notification.</summary>
    Both,
}
