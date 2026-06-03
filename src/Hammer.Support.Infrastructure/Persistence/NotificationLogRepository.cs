using Hammer.Support.Application.Abstractions;
using Hammer.Support.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Support.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="INotificationLogRepository"/>.
/// </summary>
internal sealed class NotificationLogRepository : INotificationLogRepository
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationLogRepository"/> class.
    /// </summary>
    /// <param name="db">The application database context.</param>
    public NotificationLogRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task SaveAsync(NotificationLog log, CancellationToken cancellationToken = default)
    {
        log.Id = Guid.NewGuid();
        log.CreatedAt = DateTimeOffset.UtcNow;

        _db.NotificationLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(NotificationLog log, CancellationToken cancellationToken = default)
    {
        _db.NotificationLogs.Update(log);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationLog>> GetByRecipientAsync(
        string recipientToken,
        int limit,
        Guid? sinceId,
        CancellationToken cancellationToken = default)
    {
        if (sinceId is null)
        {
            return await _db.NotificationLogs
                .AsNoTracking()
                .Where(l => l.RecipientToken == recipientToken)
                .OrderByDescending(l => l.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        DateTimeOffset? maybeSinceCreatedAt = await _db.NotificationLogs
            .Where(l => l.Id == sinceId.Value)
            .Select(l => (DateTimeOffset?)l.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (maybeSinceCreatedAt is null)
            return Array.Empty<NotificationLog>();

        DateTimeOffset sinceCreatedAt = maybeSinceCreatedAt.Value;

        return await _db.NotificationLogs
            .AsNoTracking()
            .Where(l => l.RecipientToken == recipientToken && l.CreatedAt > sinceCreatedAt)
            .OrderBy(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
