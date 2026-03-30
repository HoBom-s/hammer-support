using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;

namespace Hammer.Support.Infrastructure.Notification;

/// <summary>
///     Application service for notification template CRUD operations.
/// </summary>
internal sealed class NotificationTemplateService : INotificationTemplateService
{
    private readonly INotificationTemplateRepository _repository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationTemplateService" /> class.
    /// </summary>
    /// <param name="repository">The notification template repository.</param>
    public NotificationTemplateService(INotificationTemplateRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<NotificationTemplate>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _repository.GetAllAsync(cancellationToken);

    /// <inheritdoc />
    public Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, cancellationToken);

    /// <inheritdoc />
    public async Task<NotificationTemplate> CreateAsync(CreateNotificationTemplateCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        NotificationTemplate entity = new()
        {
            TemplateKey = command.TemplateKey,
            TitleTemplate = command.TitleTemplate,
            BodyTemplate = command.BodyTemplate,
            Channel = command.Channel,
        };

        return await _repository.CreateAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<NotificationTemplate?> UpdateAsync(Guid id, UpdateNotificationTemplateCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        NotificationTemplate? existing = await _repository.GetByIdAsync(id, cancellationToken);

        if (existing is null)
            return null;

        existing.TemplateKey = command.TemplateKey;
        existing.TitleTemplate = command.TitleTemplate;
        existing.BodyTemplate = command.BodyTemplate;
        existing.Channel = command.Channel;

        return await _repository.UpdateAsync(existing, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        _repository.DeleteAsync(id, cancellationToken);
}
