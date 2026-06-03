using Hammer.Support.Application.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// HTTP client for retrieving notification templates from hammer-internal.
/// </summary>
public interface INotificationTemplateClient
{
    /// <summary>
    /// Gets a template by its unique key from hammer-internal.
    /// </summary>
    /// <param name="templateKey">The unique template key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching template DTO, or <c>null</c> if not found.</returns>
    public Task<NotificationTemplateDto?> GetByKeyAsync(string templateKey, CancellationToken cancellationToken = default);
}
