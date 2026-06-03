using FluentAssertions;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Hammer.Support.Tests.Infrastructure.Persistence;

public sealed class NewsArticleRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly NewsArticleRepository _sut;

    public NewsArticleRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .ReplaceService<IModelCustomizer, SqliteDateTimeOffsetModelCustomizer>()
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new NewsArticleRepository(_db);
    }

    [Fact]
    public async Task GetLatestAsync_ReturnsNewestByPubDateFirst()
    {
        await SeedArticlesAsync("경매", 3);

        IReadOnlyList<NewsArticle> result = await _sut.GetLatestAsync(10);

        result.Should().HaveCount(3);
        result.Should().BeInDescendingOrder(a => a.PubDate);
    }

    [Fact]
    public async Task GetLatestAsync_RespectsLimit()
    {
        await SeedArticlesAsync("경매", 5);

        IReadOnlyList<NewsArticle> result = await _sut.GetLatestAsync(2);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLatestAsync_NoArticles_ReturnsEmpty()
    {
        IReadOnlyList<NewsArticle> result = await _sut.GetLatestAsync(10);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddRangeAsync_PersistsAllArticles()
    {
        var articles = new List<NewsArticle>
        {
            CreateArticle("부동산", "https://example.com/1", DateTimeOffset.UtcNow),
            CreateArticle("부동산", "https://example.com/2", DateTimeOffset.UtcNow),
        };

        await _sut.AddRangeAsync(articles);

        IReadOnlyList<NewsArticle> stored = await _sut.GetLatestAsync(10);
        stored.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExistsAsync_ExistingOriginalLink_ReturnsTrue()
    {
        await _sut.AddRangeAsync([CreateArticle("경매", "https://example.com/article", DateTimeOffset.UtcNow)]);

        var exists = await _sut.ExistsAsync("https://example.com/article");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_UnknownOriginalLink_ReturnsFalse()
    {
        var exists = await _sut.ExistsAsync("https://example.com/missing");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsArticle()
    {
        NewsArticle article = CreateArticle("경매", "https://example.com/article", DateTimeOffset.UtcNow);
        await _sut.AddRangeAsync([article]);

        NewsArticle? result = await _sut.GetByIdAsync(article.Id);

        result.Should().NotBeNull();
        result!.OriginalLink.Should().Be("https://example.com/article");
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        NewsArticle? result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPageAndTotalCount()
    {
        await SeedArticlesAsync("경매", 5);

        (IReadOnlyList<NewsArticle> items, var totalCount) = await _sut.GetPagedAsync(2, 2);

        items.Should().HaveCount(2);
        totalCount.Should().Be(5);
        items.Should().BeInDescendingOrder(a => a.PubDate);
    }

    [Fact]
    public async Task SearchByTitleAsync_MatchesTitleSubstring()
    {
        await _sut.AddRangeAsync(
        [
            CreateArticleWithTitle("경매 속보", "https://example.com/1"),
            CreateArticleWithTitle("부동산 동향", "https://example.com/2"),
            CreateArticleWithTitle("아파트 경매 분석", "https://example.com/3"),
        ]);

        (IReadOnlyList<NewsArticle> items, var totalCount) = await _sut.SearchByTitleAsync("경매", 1, 20);

        totalCount.Should().Be(2);
        items.Should().OnlyContain(a => a.Title.Contains("경매", StringComparison.Ordinal));
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static NewsArticle CreateArticle(string query, string originalLink, DateTimeOffset pubDate)
    {
        return new NewsArticle
        {
            Id = Guid.NewGuid(),
            Query = query,
            Title = "Title",
            OriginalLink = originalLink,
            Link = originalLink,
            Description = "Description",
            PubDate = pubDate,
            CollectedAt = DateTimeOffset.UtcNow,
        };
    }

    private static NewsArticle CreateArticleWithTitle(string title, string originalLink)
    {
        return new NewsArticle
        {
            Id = Guid.NewGuid(),
            Query = "경매",
            Title = title,
            OriginalLink = originalLink,
            Link = originalLink,
            Description = "Description",
            PubDate = DateTimeOffset.UtcNow,
            CollectedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Seeds N articles with 1-second pub-date gaps, returning them in chronological order (oldest first).
    /// </summary>
    private async Task<List<NewsArticle>> SeedArticlesAsync(string query, int count)
    {
        DateTimeOffset baseTime = DateTimeOffset.UtcNow.AddMinutes(-count);
        var articles = new List<NewsArticle>();

        for (var i = 0; i < count; i++)
            articles.Add(CreateArticle(query, $"https://example.com/{query}/{i}", baseTime.AddSeconds(i)));

        _db.NewsArticles.AddRange(articles);
        await _db.SaveChangesAsync();
        return articles;
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
