using Hammer.Support.Application.Abstractions;
using Hammer.Support.Domain.Models;

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
}
