namespace Hammer.Support.Application.Models;

/// <summary>
/// DTO representing a notification template from hammer-internal.
/// </summary>
public sealed record NotificationTemplateDto(
    Guid Id,
    string TemplateKey,
    string TitleTemplate,
    string BodyTemplate,
    string Channel);
