using Hammer.Support.Application;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Onbid;
using Hammer.Support.Infrastructure.Onbid.Institution;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hammer.Support.Tests.Infrastructure.Onbid;

public sealed class CollectInstitutionAuctionsUseCaseTests
{
    private readonly IInstitutionApiClient _client = Substitute.For<IInstitutionApiClient>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();

    private readonly OnbidOptions _options = new() { PageSize = 2 };

    [Fact]
    public async Task ExecuteAsync_PublishesAllItemsAcrossPages()
    {
        InstitutionAuctionItem item1 = new() { PlnmNo = 1, PbctNo = 10, PlnmNm = "Item1" };
        InstitutionAuctionItem item2 = new() { PlnmNo = 2, PbctNo = 20, PlnmNm = "Item2" };
        InstitutionAuctionItem item3 = new() { PlnmNo = 3, PbctNo = 30, PlnmNm = "Item3" };

        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new InstitutionPageResult { TotalCount = 3, Items = [item1, item2] });
        _client.FetchPageAsync(2, 2, Arg.Any<CancellationToken>())
            .Returns(new InstitutionPageResult { TotalCount = 3, Items = [item3] });

        CollectInstitutionAuctionsUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.False(result.Skipped);
        Assert.Equal(3, result.Processed);
        await _publisher.Received(3).PublishAsync(
            KafkaTopics.InstitutionAuction,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyFirstPage_PublishesNothing()
    {
        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new InstitutionPageResult { TotalCount = 0, Items = [] });

        CollectInstitutionAuctionsUseCase useCase = CreateUseCase();

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

        CollectInstitutionAuctionsUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Equal(0, result.Processed);
    }

    [Fact]
    public async Task ExecuteAsync_UsesCorrectKafkaKey()
    {
        InstitutionAuctionItem item = new() { PlnmNo = 10, PbctNo = 20 };

        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new InstitutionPageResult { TotalCount = 1, Items = [item] });

        CollectInstitutionAuctionsUseCase useCase = CreateUseCase();

        await useCase.ExecuteAsync(CancellationToken.None);

        await _publisher.Received(1).PublishAsync(
            KafkaTopics.InstitutionAuction,
            "10-20",
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SerializesItemAsCamelCaseJson()
    {
        InstitutionAuctionItem item = new() { PlnmNo = 1, PbctNo = 2, PlnmNm = "test-item" };

        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new InstitutionPageResult { TotalCount = 1, Items = [item] });

        var capturedValue = string.Empty;
        await _publisher.PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string>(v => capturedValue = v),
            Arg.Any<CancellationToken>());

        CollectInstitutionAuctionsUseCase useCase = CreateUseCase();

        await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Contains("\"plnmNo\":1", capturedValue, StringComparison.Ordinal);
        Assert.Contains("\"plnmNm\":\"test-item\"", capturedValue, StringComparison.Ordinal);
    }

    private CollectInstitutionAuctionsUseCase CreateUseCase()
    {
        return new CollectInstitutionAuctionsUseCase(
            _client,
            _publisher,
            Options.Create(_options),
            NullLogger<CollectInstitutionAuctionsUseCase>.Instance);
    }
}
