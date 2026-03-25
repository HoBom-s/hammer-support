using System.Diagnostics.CodeAnalysis;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Onbid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hammer.Support.Tests.Infrastructure.Onbid;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Test lifecycle managed by xUnit")]
public sealed class KamcoCollectionJobTests
{
    private readonly IKamcoApiClient _client = Substitute.For<IKamcoApiClient>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();

    private readonly OnbidOptions _options = new() { PageSize = 2 };

    [Fact]
    public async Task RunCollectionAsync_PublishesAllItemsAcrossPages()
    {
        KamcoAuctionItem item1 = new() { PlnmNo = 1, PbctNo = 10, CltrNo = 100, CltrNm = "Item1" };
        KamcoAuctionItem item2 = new() { PlnmNo = 2, PbctNo = 20, CltrNo = 200, CltrNm = "Item2" };
        KamcoAuctionItem item3 = new() { PlnmNo = 3, PbctNo = 30, CltrNo = 300, CltrNm = "Item3" };

        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new KamcoPageResult { TotalCount = 3, Items = [item1, item2] });
        _client.FetchPageAsync(2, 2, Arg.Any<CancellationToken>())
            .Returns(new KamcoPageResult { TotalCount = 3, Items = [item3] });

        KamcoCollectionJob job = CreateJob();

        await job.RunCollectionAsync(CancellationToken.None);

        await _publisher.Received(3).PublishAsync(
            "onbid-kamco-auction",
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunCollectionAsync_EmptyFirstPage_PublishesNothing()
    {
        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new KamcoPageResult { TotalCount = 0, Items = [] });

        KamcoCollectionJob job = CreateJob();

        await job.RunCollectionAsync(CancellationToken.None);

        await _publisher.DidNotReceive().PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunCollectionAsync_ClientThrows_LogsErrorAndCompletes()
    {
        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        KamcoCollectionJob job = CreateJob();

        Exception? exception = await Record.ExceptionAsync(() => job.RunCollectionAsync(CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task RunCollectionAsync_UsesCorrectKafkaKey()
    {
        KamcoAuctionItem item = new() { PlnmNo = 10, PbctNo = 20, CltrNo = 30 };

        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new KamcoPageResult { TotalCount = 1, Items = [item] });

        KamcoCollectionJob job = CreateJob();

        await job.RunCollectionAsync(CancellationToken.None);

        await _publisher.Received(1).PublishAsync(
            "onbid-kamco-auction",
            "10-20-30",
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunCollectionAsync_SerializesItemAsCamelCaseJson()
    {
        KamcoAuctionItem item = new() { PlnmNo = 1, PbctNo = 2, CltrNo = 3, CltrNm = "test-item" };

        _client.FetchPageAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(new KamcoPageResult { TotalCount = 1, Items = [item] });

        var capturedValue = string.Empty;
        await _publisher.PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string>(v => capturedValue = v),
            Arg.Any<CancellationToken>());

        KamcoCollectionJob job = CreateJob();

        await job.RunCollectionAsync(CancellationToken.None);

        Assert.Contains("\"plnmNo\":1", capturedValue, StringComparison.Ordinal);
        Assert.Contains("\"cltrNm\":\"test-item\"", capturedValue, StringComparison.Ordinal);
    }

    private KamcoCollectionJob CreateJob()
    {
        IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IKamcoApiClient)).Returns(_client);

        IServiceScope scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        IServiceScopeFactory scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        return new KamcoCollectionJob(
            scopeFactory,
            _publisher,
            Options.Create(_options),
            NullLogger<KamcoCollectionJob>.Instance);
    }
}
