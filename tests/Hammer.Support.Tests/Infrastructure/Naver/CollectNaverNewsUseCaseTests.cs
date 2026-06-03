using FluentAssertions;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Naver;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hammer.Support.Tests.Infrastructure.Naver;

public sealed class CollectNaverNewsUseCaseTests
{
    private readonly INaverNewsApiClient _apiClient = Substitute.For<INaverNewsApiClient>();
    private readonly INewsArticleRepository _repository = Substitute.For<INewsArticleRepository>();

    private readonly NaverOptions _options = new() { Keywords = ["경매", "부동산"], Display = 100 };

    [Fact]
    public async Task ExecuteAsync_SavesNewArticlesForEachKeyword()
    {
        _apiClient.SearchAsync("경매", 100, Arg.Any<CancellationToken>())
            .Returns([CreateArticle("https://example.com/a")]);
        _apiClient.SearchAsync("부동산", 100, Arg.Any<CancellationToken>())
            .Returns([CreateArticle("https://example.com/b"), CreateArticle("https://example.com/c")]);

        CollectNaverNewsUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync(CancellationToken.None);

        result.Skipped.Should().BeFalse();
        result.TotalCount.Should().Be(3);
        result.Processed.Should().Be(3);
        await _repository.Received(2).AddRangeAsync(Arg.Any<IEnumerable<NewsArticle>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SkipsDuplicatesByOriginalLink()
    {
        _apiClient.SearchAsync("경매", 100, Arg.Any<CancellationToken>())
            .Returns([CreateArticle("https://example.com/dup"), CreateArticle("https://example.com/new")]);
        _apiClient.SearchAsync("부동산", 100, Arg.Any<CancellationToken>())
            .Returns([]);

        _repository.ExistsAsync("https://example.com/dup", Arg.Any<CancellationToken>()).Returns(true);
        _repository.ExistsAsync("https://example.com/new", Arg.Any<CancellationToken>()).Returns(false);

        CollectNaverNewsUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync(CancellationToken.None);

        result.Processed.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_AllDuplicates_DoesNotCallAddRange()
    {
        _apiClient.SearchAsync(Arg.Any<string>(), 100, Arg.Any<CancellationToken>())
            .Returns([CreateArticle("https://example.com/dup")]);
        _repository.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        CollectNaverNewsUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync(CancellationToken.None);

        result.Processed.Should().Be(0);
        await _repository.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<NewsArticle>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SetsQueryAndCollectedAtOnSavedArticles()
    {
        _apiClient.SearchAsync("경매", 100, Arg.Any<CancellationToken>())
            .Returns([CreateArticle("https://example.com/a")]);
        _apiClient.SearchAsync("부동산", 100, Arg.Any<CancellationToken>())
            .Returns([]);

        List<NewsArticle> captured = [];
        await _repository.AddRangeAsync(
            Arg.Do<IEnumerable<NewsArticle>>(a => captured.AddRange(a)),
            Arg.Any<CancellationToken>());

        CollectNaverNewsUseCase useCase = CreateUseCase();

        await useCase.ExecuteAsync(CancellationToken.None);

        captured.Should().ContainSingle();
        captured[0].Query.Should().Be("경매");
        captured[0].Id.Should().NotBe(Guid.Empty);
        captured[0].CollectedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task ExecuteAsync_ApiClientThrows_ContinuesWithNextKeyword()
    {
        _apiClient.SearchAsync("경매", 100, Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("rate limited"));
        _apiClient.SearchAsync("부동산", 100, Arg.Any<CancellationToken>())
            .Returns([CreateArticle("https://example.com/b")]);

        CollectNaverNewsUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync(CancellationToken.None);

        result.Processed.Should().Be(1);
    }

    private static NewsArticle CreateArticle(string originalLink)
    {
        return new NewsArticle
        {
            Title = "Title",
            OriginalLink = originalLink,
            Link = originalLink,
            Description = "Description",
            PubDate = DateTimeOffset.UtcNow,
        };
    }

    private CollectNaverNewsUseCase CreateUseCase()
    {
        return new CollectNaverNewsUseCase(
            _apiClient,
            _repository,
            Options.Create(_options),
            NullLogger<CollectNaverNewsUseCase>.Instance);
    }
}
