using Hammer.Support.Application.Abstractions;
using Hammer.Support.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Support.Infrastructure.Persistence;

/// <summary>
///     EF Core implementation of <see cref="INotificationTemplateRepository" />.
/// </summary>
internal sealed class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly AppDbContext _db;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationTemplateRepository" /> class.
    /// </summary>
    /// <param name="db">The application database context.</param>
    public NotificationTemplateRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<NotificationTemplate?> GetByKeyAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        return await _db.NotificationTemplates
            .FirstOrDefaultAsync(t => t.TemplateKey == templateKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.NotificationTemplates.FindAsync([id], cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.NotificationTemplates
            .OrderBy(t => t.TemplateKey)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<NotificationTemplate> CreateAsync(NotificationTemplate entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        _db.NotificationTemplates.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc />
    public async Task<NotificationTemplate> UpdateAsync(NotificationTemplate entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        _db.NotificationTemplates.Update(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _db.NotificationTemplates
            .Where(t => t.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }
}
