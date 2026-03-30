namespace Hammer.Support.Domain.Models;

/// <summary>
///     Notification delivery channel extension class.
/// </summary>
public static class NotificationChannelExtensions
{
    /// <summary>
    ///     Resolves a channel into individual delivery channels.
    ///     <see cref="NotificationChannel.Both"/> is expanded to Fcm + InApp.
    /// </summary>
    /// <param name="channel">Notification channel.</param>
    /// <returns>Individual channels to deliver on.</returns>
    public static IReadOnlyList<NotificationChannel> Resolve(this NotificationChannel channel)
    {
        return channel switch
        {
            NotificationChannel.Both => [NotificationChannel.Fcm, NotificationChannel.InApp],
            _ => [channel],
        };
    }
}
