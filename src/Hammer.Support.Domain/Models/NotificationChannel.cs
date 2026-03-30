namespace Hammer.Support.Domain.Models;

/// <summary>
///     Notification delivery channel.
/// </summary>
public enum NotificationChannel
{
    /// <summary>Firebase Cloud Messaging push notification.</summary>
    Fcm,

    /// <summary>In-app notification stored in the database.</summary>
    InApp,

    /// <summary>Both FCM push and in-app notification.</summary>
    Both,
}
