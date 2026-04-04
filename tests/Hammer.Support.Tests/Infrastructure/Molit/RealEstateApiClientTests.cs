using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Molit;
using Hammer.Support.Infrastructure.Molit.Dto;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Tests.Infrastructure.Molit;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Test lifecycle managed by xUnit")]
public sealed class RealEstateApiClientTests
{
    private static readonly MolitOptions _defaultOptions = new()
    {
        ServiceKey = "test-key",
        BaseUrl = "http://localhost",
        PageSize = 100,
    };

    [Fact]
    public void MapToDomain_ApartmentItem_MapsCorrectly()
    {
        MolitTradeItem dto = new()
        {
            SggCd = "11680",
            UmdNm = " 역삼동 ",
            Jibun = " 123-4 ",
            AptNm = " 래미안 ",
            DealAmount = " 120,000 ",
            DealYear = "2026",
            DealMonth = "3",
            DealDay = "15",
            ExcluUseAr = "84.95",
            Floor = "12",
            BuildYear = "2015",
        };

        RealEstateTrade result = RealEstateApiClient.MapToDomain(dto, "11680", PropertyType.Apartment);

        Assert.Equal("11680", result.LawdCd);
        Assert.Equal(PropertyType.Apartment, result.PropertyType);
        Assert.Equal("래미안", result.BuildingName);
        Assert.Equal("123-4", result.Jibun);
        Assert.Equal("역삼동", result.UmdNm);
        Assert.Equal(120000L, result.DealAmount);
        Assert.Equal(2026, result.DealYear);
        Assert.Equal(3, result.DealMonth);
        Assert.Equal(15, result.DealDay);
        Assert.Equal(84.95m, result.Area);
        Assert.Equal(12, result.Floor);
        Assert.Equal(2015, result.BuildYear);
    }

    [Fact]
    public void MapToDomain_LandItem_HasNoFloorOrBuildYear()
    {
        MolitTradeItem dto = new()
        {
            SggCd = "11680",
            UmdNm = "역삼동",
            Jibun = "456",
            DealAmount = "50,000",
            DealYear = "2026",
            DealMonth = "2",
            DealDay = "10",
            DealArea = "330.5",
        };

        RealEstateTrade result = RealEstateApiClient.MapToDomain(dto, "11680", PropertyType.Land);

        Assert.Equal(PropertyType.Land, result.PropertyType);
        Assert.Null(result.BuildingName);
        Assert.Equal(330.5m, result.Area);
        Assert.Null(result.Floor);
        Assert.Null(result.BuildYear);
    }

    [Fact]
    public void MapToDomain_RowHouseItem_UsesMhouseNm()
    {
        MolitTradeItem dto = new()
        {
            SggCd = "11680",
            UmdNm = "역삼동",
            Jibun = "789",
            MhouseNm = "삼성빌라",
            DealAmount = "30,000",
            DealYear = "2026",
            DealMonth = "1",
            DealDay = "5",
            ExcluUseAr = "59.9",
            Floor = "3",
            BuildYear = "2000",
        };

        RealEstateTrade result = RealEstateApiClient.MapToDomain(dto, "11680", PropertyType.RowHouse);

        Assert.Equal("삼성빌라", result.BuildingName);
        Assert.Equal(59.9m, result.Area);
    }

    [Fact]
    public void MapToDomain_OfficetelItem_UsesOffiNm()
    {
        MolitTradeItem dto = new()
        {
            SggCd = "11680",
            UmdNm = "역삼동",
            Jibun = "100",
            OffiNm = "역삼오피스텔",
            DealAmount = "40,000",
            DealYear = "2026",
            DealMonth = "3",
            DealDay = "20",
            ExcluUseAr = "33.5",
            Floor = "8",
            BuildYear = "2018",
        };

        RealEstateTrade result = RealEstateApiClient.MapToDomain(dto, "11680", PropertyType.Officetel);

        Assert.Equal("역삼오피스텔", result.BuildingName);
        Assert.Equal(33.5m, result.Area);
        Assert.Equal(8, result.Floor);
    }

    [Fact]
    public void MapToDomain_DetachedItem_UsesTotalFloorAr()
    {
        MolitTradeItem dto = new()
        {
            SggCd = "11680",
            UmdNm = "역삼동",
            Jibun = "200",
            DealAmount = "80,000",
            DealYear = "2026",
            DealMonth = "2",
            DealDay = "1",
            TotalFloorAr = "198.5",
            BuildYear = "1995",
        };

        RealEstateTrade result = RealEstateApiClient.MapToDomain(dto, "11680", PropertyType.Detached);

        Assert.Null(result.BuildingName);
        Assert.Equal(198.5m, result.Area);
        Assert.Equal(1995, result.BuildYear);
    }

    [Fact]
    public void MapToDomain_CommercialItem_UsesBuildingAr()
    {
        MolitTradeItem dto = new()
        {
            SggCd = "11680",
            UmdNm = "역삼동",
            Jibun = "300",
            DealAmount = "200,000",
            DealYear = "2026",
            DealMonth = "3",
            DealDay = "5",
            BuildingAr = "150.3",
            Floor = "1",
            BuildYear = "2010",
        };

        RealEstateTrade result = RealEstateApiClient.MapToDomain(dto, "11680", PropertyType.Commercial);

        Assert.Null(result.BuildingName);
        Assert.Equal(150.3m, result.Area);
        Assert.Equal(1, result.Floor);
    }

    [Fact]
    public void MapToDomain_DealAmountWithCommasAndSpaces_ParsesCorrectly()
    {
        MolitTradeItem dto = new()
        {
            SggCd = "11680",
            UmdNm = "역삼동",
            Jibun = "1",
            DealAmount = "    12,500    ",
            DealYear = "2026",
            DealMonth = "3",
            DealDay = "1",
        };

        RealEstateTrade result = RealEstateApiClient.MapToDomain(dto, "11680", PropertyType.Apartment);

        Assert.Equal(12500L, result.DealAmount);
    }

    [Fact]
    public void MapToDomain_EmptyBuildingName_ReturnsNull()
    {
        MolitTradeItem dto = new()
        {
            SggCd = "11680",
            UmdNm = "역삼동",
            Jibun = "1",
            AptNm = "   ",
            DealAmount = "10,000",
            DealYear = "2026",
            DealMonth = "1",
            DealDay = "1",
        };

        RealEstateTrade result = RealEstateApiClient.MapToDomain(dto, "11680", PropertyType.Apartment);

        Assert.Null(result.BuildingName);
    }

    [Fact]
    public async Task FetchPageAsync_ValidApartmentResponse_ReturnsMappedTrades()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <response>
                <header>
                    <resultCode>000</resultCode>
                    <resultMsg>NORMAL SERVICE.</resultMsg>
                </header>
                <body>
                    <totalCount>1</totalCount>
                    <items>
                        <item>
                            <sggCd>11680</sggCd>
                            <umdNm>역삼동</umdNm>
                            <jibun>123-4</jibun>
                            <aptNm>래미안</aptNm>
                            <dealAmount>120,000</dealAmount>
                            <dealYear>2026</dealYear>
                            <dealMonth>3</dealMonth>
                            <dealDay>15</dealDay>
                            <excluUseAr>84.95</excluUseAr>
                            <floor>12</floor>
                            <buildYear>2015</buildYear>
                        </item>
                    </items>
                </body>
            </response>
            """;

        RealEstateApiClient client = CreateClient(xml);

        RealEstatePageResult result = await client.FetchPageAsync(
            PropertyType.Apartment,
            "11680",
            "202603",
            1,
            100);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);

        RealEstateTrade trade = result.Items[0];
        Assert.Equal("11680", trade.LawdCd);
        Assert.Equal(PropertyType.Apartment, trade.PropertyType);
        Assert.Equal("래미안", trade.BuildingName);
        Assert.Equal(120000L, trade.DealAmount);
        Assert.Equal(84.95m, trade.Area);
        Assert.Equal(12, trade.Floor);
    }

    [Fact]
    public async Task FetchPageAsync_MultipleItems_ReturnsAll()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <response>
                <header>
                    <resultCode>000</resultCode>
                    <resultMsg>NORMAL SERVICE.</resultMsg>
                </header>
                <body>
                    <totalCount>2</totalCount>
                    <items>
                        <item>
                            <sggCd>11680</sggCd>
                            <umdNm>역삼동</umdNm>
                            <jibun>1</jibun>
                            <aptNm>A</aptNm>
                            <dealAmount>100,000</dealAmount>
                            <dealYear>2026</dealYear>
                            <dealMonth>3</dealMonth>
                            <dealDay>1</dealDay>
                            <excluUseAr>84.0</excluUseAr>
                            <floor>10</floor>
                            <buildYear>2010</buildYear>
                        </item>
                        <item>
                            <sggCd>11680</sggCd>
                            <umdNm>삼성동</umdNm>
                            <jibun>2</jibun>
                            <aptNm>B</aptNm>
                            <dealAmount>90,000</dealAmount>
                            <dealYear>2026</dealYear>
                            <dealMonth>3</dealMonth>
                            <dealDay>2</dealDay>
                            <excluUseAr>59.0</excluUseAr>
                            <floor>5</floor>
                            <buildYear>2015</buildYear>
                        </item>
                    </items>
                </body>
            </response>
            """;

        RealEstateApiClient client = CreateClient(xml);

        RealEstatePageResult result = await client.FetchPageAsync(
            PropertyType.Apartment,
            "11680",
            "202603",
            1,
            100);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("역삼동", result.Items[0].UmdNm);
        Assert.Equal("삼성동", result.Items[1].UmdNm);
    }

    [Fact]
    public async Task FetchPageAsync_ErrorResponse_ReturnsEmptyResult()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <response>
                <header>
                    <resultCode>12</resultCode>
                    <resultMsg>NO OPENAPI SERVICE ERROR</resultMsg>
                </header>
                <body>
                    <totalCount>0</totalCount>
                    <items/>
                </body>
            </response>
            """;

        RealEstateApiClient client = CreateClient(xml);

        RealEstatePageResult result = await client.FetchPageAsync(
            PropertyType.Apartment,
            "11680",
            "202603",
            1,
            100);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task FetchPageAsync_EmptyItems_ReturnsEmptyResult()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <response>
                <header>
                    <resultCode>000</resultCode>
                    <resultMsg>NORMAL SERVICE.</resultMsg>
                </header>
                <body>
                    <totalCount>0</totalCount>
                    <items/>
                </body>
            </response>
            """;

        RealEstateApiClient client = CreateClient(xml);

        RealEstatePageResult result = await client.FetchPageAsync(
            PropertyType.Land,
            "11680",
            "202603",
            1,
            100);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task FetchPageAsync_UnsupportedPropertyType_ReturnsEmptyResult()
    {
        RealEstateApiClient client = CreateClient("<response/>");

        RealEstatePageResult result = await client.FetchPageAsync(
            PropertyType.Unknown,
            "11680",
            "202603",
            1,
            100);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task FetchPageAsync_LandResponse_MapsAreaFromDealArea()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <response>
                <header>
                    <resultCode>000</resultCode>
                    <resultMsg>NORMAL SERVICE.</resultMsg>
                </header>
                <body>
                    <totalCount>1</totalCount>
                    <items>
                        <item>
                            <sggCd>11680</sggCd>
                            <umdNm>역삼동</umdNm>
                            <jibun>789</jibun>
                            <dealAmount>50,000</dealAmount>
                            <dealYear>2026</dealYear>
                            <dealMonth>1</dealMonth>
                            <dealDay>10</dealDay>
                            <dealArea>330.5</dealArea>
                        </item>
                    </items>
                </body>
            </response>
            """;

        RealEstateApiClient client = CreateClient(xml);

        RealEstatePageResult result = await client.FetchPageAsync(
            PropertyType.Land,
            "11680",
            "202601",
            1,
            100);

        Assert.Single(result.Items);

        RealEstateTrade trade = result.Items[0];
        Assert.Equal(PropertyType.Land, trade.PropertyType);
        Assert.Equal(330.5m, trade.Area);
        Assert.Null(trade.BuildingName);
        Assert.Null(trade.Floor);
    }

    private static RealEstateApiClient CreateClient(string xmlResponse)
    {
        FakeHttpMessageHandler handler = new(xmlResponse);
        HttpClient httpClient = new(handler);
        IOptions<MolitOptions> options = Options.Create(_defaultOptions);

        return new RealEstateApiClient(httpClient, options, NullLogger<RealEstateApiClient>.Instance);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;

        public FakeHttpMessageHandler(string responseContent)
        {
            _responseContent = responseContent;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "application/xml"),
            };
            return Task.FromResult(response);
        }
    }
}
