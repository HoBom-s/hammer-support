using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Molit.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Molit;

/// <summary>
///     HTTP client for fetching real estate trade data from the MOLIT API.
///     Uses a single superset DTO to handle all 6 API variants.
/// </summary>
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Batch job, logging overhead negligible")]
public sealed class RealEstateApiClient : IRealEstateApiClient
{
    private static readonly XmlReaderSettings _readerSettings = new() { DtdProcessing = DtdProcessing.Prohibit, Async = true };

    private static readonly XmlSerializer _serializer = new(typeof(MolitResponse));

    /// <summary>
    ///     Maps property type to (service name, operation name).
    /// </summary>
    private static readonly Dictionary<PropertyType, (string Service, string Operation)> _endpointMap = new()
    {
        [PropertyType.Apartment] = ("RTMSDataSvcAptTrade", "getRTMSDataSvcAptTrade"),
        [PropertyType.Detached] = ("RTMSDataSvcSHTrade", "getRTMSDataSvcSHTrade"),
        [PropertyType.RowHouse] = ("RTMSDataSvcRHTrade", "getRTMSDataSvcRHTrade"),
        [PropertyType.Officetel] = ("RTMSDataSvcOffiTrade", "getRTMSDataSvcOffiTrade"),
        [PropertyType.Land] = ("RTMSDataSvcLandTrade", "getRTMSDataSvcLandTrade"),
        [PropertyType.Commercial] = ("RTMSDataSvcNrgTrade", "getRTMSDataSvcNrgTrade"),
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<RealEstateApiClient> _logger;
    private readonly MolitOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RealEstateApiClient" /> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making API requests.</param>
    /// <param name="options">The MOLIT configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public RealEstateApiClient(
        HttpClient httpClient,
        IOptions<MolitOptions> options,
        ILogger<RealEstateApiClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RealEstatePageResult> FetchPageAsync(
        PropertyType propertyType,
        string lawdCd,
        string dealYmd,
        int pageNo,
        int numOfRows,
        CancellationToken cancellationToken = default)
    {
        if (!_endpointMap.TryGetValue(propertyType, out (string Service, string Operation) endpoint))
        {
            _logger.LogWarning("Unsupported property type: {PropertyType}", propertyType);
            return new RealEstatePageResult { TotalCount = 0, Items = [] };
        }

        Uri uri = new(
            $"{_options.BaseUrl}/{endpoint.Service}/{endpoint.Operation}"
            + $"?serviceKey={_options.ServiceKey}"
            + $"&LAWD_CD={lawdCd}"
            + $"&DEAL_YMD={dealYmd}"
            + $"&pageNo={pageNo}"
            + $"&numOfRows={numOfRows}");

        _logger.LogDebug(
            "Fetching {PropertyType} trades for LAWD_CD={LawdCd}, DEAL_YMD={DealYmd}, page {PageNo}",
            propertyType,
            lawdCd,
            dealYmd,
            pageNo);

        using Stream stream = await _httpClient.GetStreamAsync(uri, cancellationToken);
        using var reader = XmlReader.Create(stream, _readerSettings);

        var response = (MolitResponse?)_serializer.Deserialize(reader);

        if (response is null || response.Header.ResultCode != "000")
        {
            _logger.LogError(
                "MOLIT API error for {PropertyType} LAWD_CD={LawdCd} DEAL_YMD={DealYmd}: {Code} {Msg}",
                propertyType,
                lawdCd,
                dealYmd,
                response?.Header.ResultCode,
                response?.Header.ResultMsg);

            return new RealEstatePageResult { TotalCount = 0, Items = [] };
        }

        IReadOnlyList<RealEstateTrade> items = response.Body.Items.ItemList
            .Select(dto => MapToDomain(dto, lawdCd, propertyType))
            .ToList();

        return new RealEstatePageResult { TotalCount = response.Body.TotalCount, Items = items };
    }

    /// <summary>
    ///     Maps a raw MOLIT XML item to a <see cref="RealEstateTrade" /> domain model.
    /// </summary>
    /// <param name="dto">The deserialized XML trade item.</param>
    /// <param name="lawdCd">The district code used in the API query.</param>
    /// <param name="propertyType">The property type used in the API query.</param>
    /// <returns>A domain trade record.</returns>
    internal static RealEstateTrade MapToDomain(MolitTradeItem dto, string lawdCd, PropertyType propertyType)
    {
        // Pick the building name based on property type.
        var buildingName = propertyType switch
        {
            PropertyType.Apartment => NullIfEmpty(dto.AptNm),
            PropertyType.RowHouse => NullIfEmpty(dto.MhouseNm),
            PropertyType.Officetel => NullIfEmpty(dto.OffiNm),
            _ => null,
        };

        // Pick the area based on property type.
        var area = propertyType switch
        {
            PropertyType.Apartment or PropertyType.RowHouse or PropertyType.Officetel
                => ParseDecimal(dto.ExcluUseAr),
            PropertyType.Detached => ParseDecimal(dto.TotalFloorAr),
            PropertyType.Land => ParseDecimal(dto.DealArea),
            PropertyType.Commercial => ParseDecimal(dto.BuildingAr),
            _ => 0m,
        };

        return new RealEstateTrade
        {
            LawdCd = lawdCd,
            PropertyType = propertyType,
            BuildingName = buildingName,
            Jibun = dto.Jibun.Trim(),
            UmdNm = dto.UmdNm.Trim(),
            DealAmount = ParseDealAmount(dto.DealAmount),
            DealYear = ParseInt(dto.DealYear),
            DealMonth = ParseInt(dto.DealMonth),
            DealDay = ParseInt(dto.DealDay),
            Area = area,
            Floor = ParseNullableInt(dto.Floor),
            BuildYear = ParseNullableInt(dto.BuildYear),
        };
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static long ParseDealAmount(string value) =>
        long.TryParse(value.Replace(",", string.Empty, StringComparison.Ordinal).Trim(), out var result) ? result : 0;

    private static decimal ParseDecimal(string value) =>
        decimal.TryParse(value.Trim(), out var result) ? result : 0m;

    private static int ParseInt(string value) =>
        int.TryParse(value.Trim(), out var result) ? result : 0;

    private static int? ParseNullableInt(string value) =>
        int.TryParse(value.Trim(), out var result) ? result : null;
}
