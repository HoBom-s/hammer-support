using Hammer.Support.Application;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Onbid;
using Hammer.Support.Infrastructure.Onbid.CodeInfo;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hammer.Support.Tests.Infrastructure.Onbid;

public sealed class CollectCodeInfoUseCaseTests
{
    private readonly ICodeInfoApiClient _client = Substitute.For<ICodeInfoApiClient>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();

    private readonly OnbidOptions _options = new() { PageSize = 2 };

    [Fact]
    public async Task ExecuteAsync_PublishesAllItemsAcrossPages()
    {
        OnbidCodeItem item1 = new() { CtgrId = "001", CtgrNm = "토지" };
        OnbidCodeItem item2 = new() { CtgrId = "002", CtgrNm = "건물" };
        OnbidCodeItem item3 = new() { CtgrId = "003", CtgrNm = "차량" };

        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new CodeInfoPageResult { TotalCount = 3, Items = [item1, item2] });
        _client.FetchPageAsync(2, 2, Arg.Any<CancellationToken>())
            .Returns(new CodeInfoPageResult { TotalCount = 3, Items = [item3] });

        CollectCodeInfoUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.False(result.Skipped);
        Assert.Equal(3, result.Processed);
        await _publisher.Received(3).PublishAsync(
            KafkaTopics.CodeInfo,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyFirstPage_PublishesNothing()
    {
        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new CodeInfoPageResult { TotalCount = 0, Items = [] });

        CollectCodeInfoUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(0, result.Processed);
        await _publisher.DidNotReceive().PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ClientThrows_ReturnsWithoutException()
    {
        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        CollectCodeInfoUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(0, result.Processed);
    }

    [Fact]
    public async Task ExecuteAsync_UsesCtgrIdAsKafkaKey()
    {
        OnbidCodeItem item = new() { CtgrId = "001", CtgrNm = "토지" };

        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new CodeInfoPageResult { TotalCount = 1, Items = [item] });

        CollectCodeInfoUseCase useCase = CreateUseCase();

        await useCase.ExecuteAsync(CancellationToken.None);

        await _publisher.Received(1).PublishAsync(
            KafkaTopics.CodeInfo,
            "001",
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SerializesItemAsCamelCaseJson()
    {
        OnbidCodeItem item = new() { CtgrId = "001", CtgrNm = "land", CtgrHirkId = "000", CtgrHirkNm = "all" };

        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new CodeInfoPageResult { TotalCount = 1, Items = [item] });

        var capturedValue = string.Empty;
        await _publisher.PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string>(v => capturedValue = v),
            Arg.Any<CancellationToken>());

        CollectCodeInfoUseCase useCase = CreateUseCase();

        await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Contains("\"ctgrId\":\"001\"", capturedValue, StringComparison.Ordinal);
        Assert.Contains("\"ctgrNm\":\"land\"", capturedValue, StringComparison.Ordinal);
        Assert.Contains("\"ctgrHirkId\":\"000\"", capturedValue, StringComparison.Ordinal);
    }

    private CollectCodeInfoUseCase CreateUseCase()
    {
        return new CollectCodeInfoUseCase(
            _client,
            _publisher,
            Options.Create(_options),
            NullLogger<CollectCodeInfoUseCase>.Instance);
    }
}
