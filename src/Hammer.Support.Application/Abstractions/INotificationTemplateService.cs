using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
///     Application service for notification template CRUD operations.
/// </summary>
public interface INotificationTemplateService
{
    /// <summary>
    ///     Gets all templates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of all templates.</returns>
    public Task<IReadOnlyList<NotificationTemplate>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a template by its ID.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching template, or <c>null</c> if not found.</returns>
    public Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new template.
    /// </summary>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created template with generated ID and timestamps.</returns>
    public Task<NotificationTemplate> CreateAsync(CreateNotificationTemplateCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing template.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="command">The update command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated template, or <c>null</c> if not found.</returns>
    public Task<NotificationTemplate?> UpdateAsync(Guid id, UpdateNotificationTemplateCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a template by ID.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the template was deleted; otherwise <c>false</c>.</returns>
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
