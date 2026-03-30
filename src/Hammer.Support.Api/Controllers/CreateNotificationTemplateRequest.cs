using System.Diagnostics.CodeAnalysis;
using Hammer.Support.Domain.Models;

namespace Hammer.Support.Api.Controllers;

/// <summary>
/// Request body for creating a notification template.
/// </summary>
[SuppressMessage("Performance", "CA1515:Consider making public types internal", Justification = "MVC model binding requires public types")]
public sealed record CreateNotificationTemplateRequest
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
