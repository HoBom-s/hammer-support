using System.Globalization;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Molit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Tests.Infrastructure.Molit;

/// <summary>
///     Integration tests that call the real MOLIT API.
///     Skipped automatically when Molit__ServiceKey is not set.
///     Run manually: Molit__ServiceKey=YOUR_KEY dotnet test --filter "FullyQualifiedName~IntegrationTests".
/// </summary>
public sealed class RealEstateApiClientIntegrationTests : IDisposable
{
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    private static string? ServiceKey =>
        Environment.GetEnvironmentVariable("Molit__ServiceKey");

    public void Dispose() =>
        _httpClient.Dispose();

    [SkippableFact]
    public async Task FetchPageAsync_Apartment_ReturnsRealData()
    {
        Skip.If(string.IsNullOrEmpty(ServiceKey), "Molit__ServiceKey not set");

        RealEstateApiClient client = CreateClient();

        // 서울 강남구(11680), 최근 월
        var dealYmd = DateTime.UtcNow.AddMonths(-1).ToString("yyyyMM", CultureInfo.InvariantCulture);

        RealEstatePageResult result = await client.FetchPageAsync(
            PropertyType.Apartment,
            "11680",
            dealYmd,
            1,
            10);

        Assert.True(result.TotalCount >= 0, "TotalCount should be non-negative");

        Assert.All(
            result.Items,
            trade =>
            {
                Assert.Equal("11680", trade.LawdCd);
                Assert.Equal(PropertyType.Apartment, trade.PropertyType);
                Assert.NotEmpty(trade.UmdNm);
                Assert.True(trade.DealAmount > 0, "DealAmount should be positive");
                Assert.True(trade.Area > 0, "Area should be positive");
            });
    }

    [SkippableFact]
    public async Task FetchPageAsync_Land_ReturnsRealData()
    {
        Skip.If(string.IsNullOrEmpty(ServiceKey), "Molit__ServiceKey not set");

        RealEstateApiClient client = CreateClient();

        var dealYmd = DateTime.UtcNow.AddMonths(-1).ToString("yyyyMM", CultureInfo.InvariantCulture);

        RealEstatePageResult result = await client.FetchPageAsync(
            PropertyType.Land,
            "11680",
            dealYmd,
            1,
            10);

        Assert.True(result.TotalCount >= 0);

        Assert.All(
            result.Items,
            trade =>
            {
                Assert.Equal(PropertyType.Land, trade.PropertyType);
                Assert.Null(trade.BuildingName);
            });
    }

    [SkippableTheory]
    [InlineData(PropertyType.Apartment)]
    [InlineData(PropertyType.Detached)]
    [InlineData(PropertyType.RowHouse)]
    [InlineData(PropertyType.Officetel)]
    [InlineData(PropertyType.Land)]
    [InlineData(PropertyType.Commercial)]
    public async Task FetchPageAsync_AllPropertyTypes_DoNotThrow(PropertyType propertyType)
    {
        Skip.If(string.IsNullOrEmpty(ServiceKey), "Molit__ServiceKey not set");

        RealEstateApiClient client = CreateClient();

        var dealYmd = DateTime.UtcNow.AddMonths(-1).ToString("yyyyMM", CultureInfo.InvariantCulture);

        RealEstatePageResult result = await client.FetchPageAsync(
            propertyType,
            "11680",
            dealYmd,
            1,
            5);

        // API should respond without throwing for all 6 property types
        Assert.True(result.TotalCount >= 0);
        Assert.NotNull(result.Items);
    }

    private RealEstateApiClient CreateClient()
    {
        MolitOptions options = new() { ServiceKey = ServiceKey!, BaseUrl = "https://apis.data.go.kr/1613000", PageSize = 10 };

        return new RealEstateApiClient(
            _httpClient,
            Options.Create(options),
            NullLogger<RealEstateApiClient>.Instance);
    }
}
