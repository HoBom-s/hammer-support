using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Repository for notification templates.
/// </summary>
public interface INotificationTemplateRepository
{
    /// <summary>
    /// Gets a template by its unique key.
    /// </summary>
    /// <param name="templateKey">The unique template key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching template, or <c>null</c> if not found.</returns>
    public Task<NotificationTemplate?> GetByKeyAsync(string templateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a template by its ID.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching template, or <c>null</c> if not found.</returns>
    public Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all templates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of all templates ordered by key.</returns>
    public Task<IReadOnlyList<NotificationTemplate>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new template.
    /// </summary>
    /// <param name="entity">The template to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created template with generated ID and timestamps.</returns>
    public Task<NotificationTemplate> CreateAsync(NotificationTemplate entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing template.
    /// </summary>
    /// <param name="entity">The template with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated template.</returns>
    public Task<NotificationTemplate> UpdateAsync(NotificationTemplate entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a template by ID.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the template was deleted; otherwise <c>false</c>.</returns>
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
