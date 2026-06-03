namespace Hammer.Support.Domain.Models;

/// <summary>
///     Record of a sent notification for audit and in-app display.
/// </summary>
public sealed class NotificationLog
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the template ID used to generate this notification.</summary>
    public Guid TemplateId { get; set; }

    /// <summary>Gets or sets the FCM device token or user identifier.</summary>
    public string RecipientToken { get; set; } = string.Empty;

    /// <summary>Gets or sets the delivery channel used.</summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>Gets or sets the rendered title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the rendered body.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Gets or sets the delivery status.</summary>
    public NotificationStatus Status { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the error message if delivery failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     Creates a new notification log entry with pending status.
    /// </summary>
    /// <param name="templateId">Template identifier.</param>
    /// <param name="recipientToken">Recipient device token or user identifier.</param>
    /// <param name="title">Rendered notification title.</param>
    /// <param name="body">Rendered notification body.</param>
    /// <param name="channel">Delivery channel.</param>
    /// <returns>A new <see cref="NotificationLog"/> instance.</returns>
    public static NotificationLog CreateLog(Guid templateId, string recipientToken, string title, string body, NotificationChannel channel)
    {
        return new NotificationLog
        {
            TemplateId = templateId,
            RecipientToken = recipientToken,
            Title = title,
            Body = body,
            Status = NotificationStatus.Pending,
            Channel = channel,
        };
    }

    /// <summary>
    ///     Change notification status to 'SENT'.
    /// </summary>
    public void ChangeNotificationStatusToSent() =>
        Status = NotificationStatus.Sent;

    /// <summary>
    ///     Change notification status to 'FAILED'.
    /// </summary>
    /// <param name="msg">Exception message.</param>
    public void ChangeNotificationStatusToFailed(string msg)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = msg;
    }
}
