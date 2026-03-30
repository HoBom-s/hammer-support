namespace Hammer.Support.Domain.Models;

/// <summary>
/// Notification message template stored in the database.
/// </summary>
public sealed class NotificationTemplate
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the template key used for lookup (e.g. "auction_new_item").</summary>
    public string TemplateKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the title template with {placeholder} variables.</summary>
    public string TitleTemplate { get; set; } = string.Empty;

    /// <summary>Gets or sets the body template with {placeholder} variables.</summary>
    public string BodyTemplate { get; set; } = string.Empty;

    /// <summary>Gets or sets the delivery channel.</summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
