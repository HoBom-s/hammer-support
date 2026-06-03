using System.Text.Json;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Molit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Hammer.Support.Tests.Infrastructure.Molit;

public sealed class CollectRealEstatePriceUseCaseTests
{
    private readonly IRealEstateApiClient _client = Substitute.For<IRealEstateApiClient>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly ILawdCodeResolver _resolver = Substitute.For<ILawdCodeResolver>();

    private readonly MolitOptions _options = new() { PageSize = 10, MonthsToCollect = 1 };

    [Fact]
    public async Task ExecuteAsync_PublishesTradesForMappedItems()
    {
        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 123", CtgrFullNm = "주거용건물 > 아파트" },
        ];

        _resolver.Resolve("서울특별시 강남구 역삼동 123").Returns("11680");

        RealEstateTrade trade = new() { LawdCd = "11680", DealAmount = 120000 };
        _client.FetchPageAsync(
                PropertyType.Apartment,
                "11680",
                Arg.Any<string>(),
                1,
                10,
                Arg.Any<CancellationToken>())
            .Returns(new RealEstatePageResult { TotalCount = 1, Items = [trade] });

        CollectRealEstatePriceUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        Assert.False(result.Skipped);
        Assert.Equal(1, result.Processed);
        await _publisher.Received(1).PublishAsync(
            "real-estate-market-price",
            "11680-Apartment",
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeduplicatesDistrictAndType()
    {
        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 123", CtgrFullNm = "아파트" },
            new() { LdnmAdrs = "서울특별시 강남구 삼성동 456", CtgrFullNm = "아파트" },
        ];

        _resolver.Resolve(Arg.Is<string>(s => s.StartsWith("서울특별시 강남구", StringComparison.Ordinal)))
            .Returns("11680");

        _client.FetchPageAsync(
                PropertyType.Apartment,
                "11680",
                Arg.Any<string>(),
                1,
                10,
                Arg.Any<CancellationToken>())
            .Returns(new RealEstatePageResult { TotalCount = 0, Items = [] });

        CollectRealEstatePriceUseCase useCase = CreateUseCase();
        await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        // Should only call once (deduplicated)
        await _client.Received(1).FetchPageAsync(
            PropertyType.Apartment,
            "11680",
            Arg.Any<string>(),
            1,
            10,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SkipsUnmappedItems()
    {
        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "알 수 없는 주소", CtgrFullNm = "선박" },
        ];

        _resolver.Resolve("알 수 없는 주소").Returns((string?)null);

        CollectRealEstatePriceUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        Assert.Equal(0, result.Processed);
        await _client.DidNotReceive().FetchPageAsync(
            Arg.Any<PropertyType>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyKamcoItems_ReturnsZero()
    {
        CollectRealEstatePriceUseCase useCase = CreateUseCase();

        CollectionResult result = await useCase.ExecuteAsync([], CancellationToken.None);

        Assert.Equal(0, result.Processed);
    }

    [Fact]
    public void GenerateDealYmds_ReturnsCorrectMonths()
    {
        List<string> result = CollectRealEstatePriceUseCase.GenerateDealYmds(3);

        Assert.Equal(3, result.Count);

        // All should be 6-digit YYYYMM strings
        Assert.All(result, s => Assert.Matches("^\\d{6}$", s));
    }

    [Fact]
    public async Task ExecuteAsync_MultiplePropertyTypes_CallsCorrectEndpoints()
    {
        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 1", CtgrFullNm = "아파트" },
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 2", CtgrFullNm = "토지" },
        ];

        _resolver.Resolve(Arg.Any<string>()).Returns("11680");

        _client.FetchPageAsync(
                Arg.Any<PropertyType>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(new RealEstatePageResult { TotalCount = 0, Items = [] });

        CollectRealEstatePriceUseCase useCase = CreateUseCase();
        await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        // Should call for both Apartment and Land
        await _client.Received().FetchPageAsync(
            PropertyType.Apartment,
            "11680",
            Arg.Any<string>(),
            1,
            10,
            Arg.Any<CancellationToken>());

        await _client.Received().FetchPageAsync(
            PropertyType.Land,
            "11680",
            Arg.Any<string>(),
            1,
            10,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MultipleMonths_CallsApiForEachMonth()
    {
        _options.MonthsToCollect = 3;

        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 1", CtgrFullNm = "아파트" },
        ];

        _resolver.Resolve(Arg.Any<string>()).Returns("11680");

        _client.FetchPageAsync(
                Arg.Any<PropertyType>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(new RealEstatePageResult { TotalCount = 0, Items = [] });

        CollectRealEstatePriceUseCase useCase = CreateUseCase();
        await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        // 1 district × 3 months = 3 API calls
        await _client.Received(3).FetchPageAsync(
            PropertyType.Apartment,
            "11680",
            Arg.Any<string>(),
            1,
            10,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Pagination_FetchesMultiplePages()
    {
        _options.PageSize = 2;

        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 1", CtgrFullNm = "아파트" },
        ];

        _resolver.Resolve(Arg.Any<string>()).Returns("11680");

        RealEstateTrade trade1 = new() { LawdCd = "11680", DealAmount = 100000 };
        RealEstateTrade trade2 = new() { LawdCd = "11680", DealAmount = 200000 };
        RealEstateTrade trade3 = new() { LawdCd = "11680", DealAmount = 300000 };

        // Page 1: 2 items (full page → more pages)
        _client.FetchPageAsync(
                PropertyType.Apartment,
                "11680",
                Arg.Any<string>(),
                1,
                2,
                Arg.Any<CancellationToken>())
            .Returns(new RealEstatePageResult { TotalCount = 3, Items = [trade1, trade2] });

        // Page 2: 1 item (partial → last page)
        _client.FetchPageAsync(
                PropertyType.Apartment,
                "11680",
                Arg.Any<string>(),
                2,
                2,
                Arg.Any<CancellationToken>())
            .Returns(new RealEstatePageResult { TotalCount = 3, Items = [trade3] });

        CollectRealEstatePriceUseCase useCase = CreateUseCase();
        CollectionResult result = await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        Assert.Equal(3, result.Processed);
        await _publisher.Received(3).PublishAsync(
            "real-estate-market-price",
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MixedMappedAndUnmapped_ProcessesOnlyMapped()
    {
        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 1", CtgrFullNm = "아파트" },
            new() { LdnmAdrs = "알 수 없는 주소", CtgrFullNm = "선박" },
            new() { LdnmAdrs = "경기도 성남시 분당구 정자동 2", CtgrFullNm = "오피스텔" },
        ];

        _resolver.Resolve("서울특별시 강남구 역삼동 1").Returns("11680");
        _resolver.Resolve("알 수 없는 주소").Returns((string?)null);
        _resolver.Resolve("경기도 성남시 분당구 정자동 2").Returns("41135");

        _client.FetchPageAsync(
                Arg.Any<PropertyType>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(new RealEstatePageResult { TotalCount = 0, Items = [] });

        CollectRealEstatePriceUseCase useCase = CreateUseCase();
        await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        // 2 mapped: (11680, Apartment) and (41135, Officetel). "선박" is Unknown → skipped.
        await _client.Received().FetchPageAsync(
            PropertyType.Apartment,
            "11680",
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());

        await _client.Received().FetchPageAsync(
            PropertyType.Officetel,
            "41135",
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_FlushReportsFailures_CountedInResult()
    {
        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 1", CtgrFullNm = "아파트" },
        ];

        _resolver.Resolve(Arg.Any<string>()).Returns("11680");

        RealEstateTrade trade = new() { LawdCd = "11680", DealAmount = 100000 };
        _client.FetchPageAsync(
                Arg.Any<PropertyType>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(new RealEstatePageResult { TotalCount = 1, Items = [trade] });

        // Simulate 1 failed delivery on flush
        _publisher.FlushAsync(Arg.Any<CancellationToken>()).Returns(1);

        CollectRealEstatePriceUseCase useCase = CreateUseCase();
        CollectionResult result = await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        Assert.Equal(1, result.Processed);
        Assert.Equal(1, result.Failed);
    }

    [Fact]
    public async Task ExecuteAsync_PublishedJson_ContainsCamelCaseProperties()
    {
        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 1", CtgrFullNm = "아파트" },
        ];

        _resolver.Resolve(Arg.Any<string>()).Returns("11680");

        RealEstateTrade trade = new()
        {
            LawdCd = "11680",
            PropertyType = PropertyType.Apartment,
            BuildingName = "래미안",
            DealAmount = 120000,
            DealYear = 2026,
            DealMonth = 3,
            DealDay = 15,
            Area = 84.95m,
        };

        _client.FetchPageAsync(
                Arg.Any<PropertyType>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(new RealEstatePageResult { TotalCount = 1, Items = [trade] });

        string? capturedJson = null;
        await _publisher.PublishAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string>(json => capturedJson = json),
            Arg.Any<CancellationToken>());

        CollectRealEstatePriceUseCase useCase = CreateUseCase();
        await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        Assert.NotNull(capturedJson);

        // Verify JSON is camelCase and contains expected data
        using var doc = JsonDocument.Parse(capturedJson);
        JsonElement root = doc.RootElement;

        Assert.Equal("11680", root.GetProperty("lawdCd").GetString());
        Assert.Equal("래미안", root.GetProperty("buildingName").GetString());
        Assert.Equal(120000, root.GetProperty("dealAmount").GetInt64());
        Assert.Equal(84.95m, root.GetProperty("area").GetDecimal());
    }

    [Fact]
    public async Task ExecuteAsync_UnknownPropertyType_SkippedWithoutApiCall()
    {
        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 1", CtgrFullNm = "차량" },
        ];

        // Address resolves, but category maps to Unknown
        _resolver.Resolve(Arg.Any<string>()).Returns("11680");

        CollectRealEstatePriceUseCase useCase = CreateUseCase();
        CollectionResult result = await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        Assert.Equal(0, result.Processed);
        await _client.DidNotReceive().FetchPageAsync(
            Arg.Any<PropertyType>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_NullAddress_SkippedWithoutApiCall()
    {
        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 1", CtgrFullNm = "아파트" },
        ];

        // Address fails to resolve
        _resolver.Resolve(Arg.Any<string>()).Returns((string?)null);

        CollectRealEstatePriceUseCase useCase = CreateUseCase();
        CollectionResult result = await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        Assert.Equal(0, result.Processed);
    }

    [Fact]
    public async Task ExecuteAsync_KafkaKey_FollowsExpectedFormat()
    {
        List<KamcoAuctionItem> kamcoItems =
        [
            new() { LdnmAdrs = "서울특별시 강남구 역삼동 1", CtgrFullNm = "토지" },
        ];

        _resolver.Resolve(Arg.Any<string>()).Returns("11680");

        RealEstateTrade trade = new() { LawdCd = "11680" };
        _client.FetchPageAsync(
                Arg.Any<PropertyType>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(new RealEstatePageResult { TotalCount = 1, Items = [trade] });

        CollectRealEstatePriceUseCase useCase = CreateUseCase();
        await useCase.ExecuteAsync(kamcoItems, CancellationToken.None);

        await _publisher.Received(1).PublishAsync(
            "real-estate-market-price",
            "11680-Land",
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    private CollectRealEstatePriceUseCase CreateUseCase()
    {
        return new CollectRealEstatePriceUseCase(
            _client,
            _publisher,
            _resolver,
            Options.Create(_options),
            NullLogger<CollectRealEstatePriceUseCase>.Instance);
    }
}
