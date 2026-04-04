using FluentAssertions;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Hammer.Support.Tests.Infrastructure.Persistence;

public sealed class NotificationLogRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly NotificationLogRepository _sut;

    public NotificationLogRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .ReplaceService<IModelCustomizer, SqliteDateTimeOffsetModelCustomizer>()
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new NotificationLogRepository(_db);
    }

    [Fact]
    public async Task GetByRecipientAsync_NullSinceId_ReturnsNewestFirst()
    {
        await SeedLogsAsync("token-a", 3);

        IReadOnlyList<NotificationLog> result = await _sut.GetByRecipientAsync("token-a", 10, null);

        result.Should().HaveCount(3);
        result.Should().BeInDescendingOrder(l => l.CreatedAt);
    }

    [Fact]
    public async Task GetByRecipientAsync_WithSinceId_ReturnsOnlyNewerLogsAscending()
    {
        List<NotificationLog> logs = await SeedLogsAsync("token-b", 5);
        Guid sinceId = logs[2].Id; // middle log

        IReadOnlyList<NotificationLog> result = await _sut.GetByRecipientAsync("token-b", 10, sinceId);

        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(l => l.CreatedAt);
        result.Should().OnlyContain(l => l.CreatedAt > logs[2].CreatedAt);
    }

    [Fact]
    public async Task GetByRecipientAsync_WithNonExistentSinceId_ReturnsEmpty()
    {
        await SeedLogsAsync("token-c", 3);

        IReadOnlyList<NotificationLog> result = await _sut.GetByRecipientAsync("token-c", 10, Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRecipientAsync_WithSinceId_RespectsLimit()
    {
        List<NotificationLog> logs = await SeedLogsAsync("token-d", 5);
        Guid sinceId = logs[0].Id; // oldest log — 4 newer exist

        IReadOnlyList<NotificationLog> result = await _sut.GetByRecipientAsync("token-d", 2, sinceId);

        result.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    /// <summary>
    /// Seeds N logs with 1-second gaps, returning them in chronological order (oldest first).
    /// </summary>
    private async Task<List<NotificationLog>> SeedLogsAsync(string recipientToken, int count)
    {
        DateTimeOffset baseTime = DateTimeOffset.UtcNow.AddMinutes(-count);
        var logs = new List<NotificationLog>();

        for (var i = 0; i < count; i++)
        {
            var log = NotificationLog.CreateLog(Guid.NewGuid(), recipientToken, $"Title {i}", $"Body {i}", NotificationChannel.InApp);
            log.Id = Guid.NewGuid();
            log.CreatedAt = baseTime.AddSeconds(i);
            logs.Add(log);
        }

        _db.NotificationLogs.AddRange(logs);
        await _db.SaveChangesAsync();
        return logs;
    }

    /// <summary>
    /// Adds DateTimeOffset-to-ticks conversion so SQLite can compare DateTimeOffset values.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by EF Core via ReplaceService")]
    private sealed class SqliteDateTimeOffsetModelCustomizer : RelationalModelCustomizer
    {
        public SqliteDateTimeOffsetModelCustomizer(ModelCustomizerDependencies dependencies)
            : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableProperty property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                        property.SetValueConverter(typeof(DateTimeOffsetToTicksConverter));
                }
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by EF Core via SetValueConverter")]
    private sealed class DateTimeOffsetToTicksConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset, long>
    {
        public DateTimeOffsetToTicksConverter()
            : base(d => d.UtcTicks, t => new DateTimeOffset(t, TimeSpan.Zero))
        {
        }
    }
}
