using FluentAssertions;
using Hammer.Support.Api.Controllers;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Hammer.Support.Tests.Api;

public sealed class NewsControllerTests
{
    private readonly INewsArticleRepository _repo = Substitute.For<INewsArticleRepository>();
    private readonly NewsController _sut;

    public NewsControllerTests()
    {
        _sut = new NewsController(_repo);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsOkWithStrippedTitles()
    {
        _repo.GetLatestAsync(5, Arg.Any<CancellationToken>())
            .Returns([CreateArticle("https://example.com/1", "<b>경매</b> 뉴스")]);

        IActionResult result = await _sut.GetRecentAsync(5, CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        IReadOnlyList<NewsResponse> items = ok.Value.Should().BeAssignableTo<IReadOnlyList<NewsResponse>>().Subject;
        items.Should().ContainSingle();
        items[0].Title.Should().Be("경매 뉴스");
    }

    [Fact]
    public async Task GetRecentAsync_CountExceeds100_ClampedTo100()
    {
        _repo.GetLatestAsync(100, Arg.Any<CancellationToken>()).Returns(new List<NewsArticle>());

        await _sut.GetRecentAsync(999, CancellationToken.None);

        await _repo.Received(1).GetLatestAsync(100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_ExistingArticle_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(CreateArticle("https://example.com/1", "제목"));

        IActionResult result = await _sut.GetByIdAsync(id, CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<NewsResponse>();
    }

    [Fact]
    public async Task GetByIdAsync_MissingArticle_ReturnsNotFound()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((NewsArticle?)null);

        IActionResult result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPagedResponseWithTotals()
    {
        _repo.GetPagedAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<NewsArticle> { CreateArticle("https://example.com/1", "t") }, 45));

        IActionResult result = await _sut.GetPagedAsync(1, 20, CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        PagedResponse<NewsResponse> paged = ok.Value.Should().BeOfType<PagedResponse<NewsResponse>>().Subject;
        paged.TotalCount.Should().Be(45);
        paged.TotalPages.Should().Be(3);
        paged.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetPagedAsync_SizeExceeds100_ClampedTo100()
    {
        _repo.GetPagedAsync(1, 100, Arg.Any<CancellationToken>())
            .Returns((new List<NewsArticle>(), 0));

        await _sut.GetPagedAsync(1, 999, CancellationToken.None);

        await _repo.Received(1).GetPagedAsync(1, 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_EmptyKeyword_ReturnsBadRequest()
    {
        IActionResult result = await _sut.SearchAsync("  ", 1, 20, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SearchAsync_ValidKeyword_ReturnsPagedResponse()
    {
        _repo.SearchByTitleAsync("경매", 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<NewsArticle> { CreateArticle("https://example.com/1", "경매 속보") }, 1));

        IActionResult result = await _sut.SearchAsync("경매", 1, 20, CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        PagedResponse<NewsResponse> paged = ok.Value.Should().BeOfType<PagedResponse<NewsResponse>>().Subject;
        paged.TotalCount.Should().Be(1);
    }

    private static NewsArticle CreateArticle(string originalLink, string title)
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
}
