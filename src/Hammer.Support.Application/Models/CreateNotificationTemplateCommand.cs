using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Models;

/// <summary>
/// Command to create a new notification template.
/// </summary>
public sealed record CreateNotificationTemplateCommand
{
    /// <summary>Gets the template key.</summary>
    public required string TemplateKey { get; init; }

    /// <summary>Gets the title template.</summary>
    public required string TitleTemplate { get; init; }

    /// <summary>Gets the body template.</summary>
    public required string BodyTemplate { get; init; }

    /// <summary>Gets the delivery channel.</summary>
    public required NotificationChannel Channel { get; init; }
}
