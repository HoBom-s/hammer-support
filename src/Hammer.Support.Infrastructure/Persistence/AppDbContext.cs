using Hammer.Support.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Support.Infrastructure.Persistence;

/// <summary>
/// Application database context for notification data.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">Database context options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets the notification logs table.</summary>
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    /// <summary>Gets the news articles table.</summary>
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
