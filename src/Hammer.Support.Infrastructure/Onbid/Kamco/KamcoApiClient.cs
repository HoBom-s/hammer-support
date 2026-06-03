using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Onbid.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Onbid.Kamco;

/// <summary>
/// HTTP client for fetching KAMCO auction items from the Onbid API.
/// </summary>
public sealed class KamcoApiClient : IKamcoApiClient
{
    private static readonly XmlSerializer _serializer = new(typeof(OnbidResponse));

    private static readonly XmlReaderSettings _readerSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
    };

    private readonly HttpClient _httpClient;
    private readonly OnbidOptions _options;
    private readonly ILogger<KamcoApiClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="KamcoApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client instance.</param>
    /// <param name="options">Onbid configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public KamcoApiClient(HttpClient httpClient, IOptions<OnbidOptions> options, ILogger<KamcoApiClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Runs once per page in daily batch")]
    public async Task<KamcoPageResult> FetchPageAsync(int pageNo, int numOfRows, CancellationToken cancellationToken = default)
    {
        // WARNING: ServiceKey is in the query string per Onbid API design.
        // Never log this URI to avoid credential exposure.
        Uri uri = new($"{_options.KamcoBaseUrl}/getKamcoPbctCltrList"
            + $"?ServiceKey={_options.ServiceKey}"
            + $"&numOfRows={numOfRows}"
            + $"&pageNo={pageNo}"
            + $"&DPSL_MTD_CD=0001");

        _logger.LogInformation("Fetching KAMCO auction page {PageNo} ({NumOfRows} rows)", pageNo, numOfRows);

        using Stream stream = await _httpClient.GetStreamAsync(uri, cancellationToken);
        using var reader = XmlReader.Create(stream, _readerSettings);
        var response = (OnbidResponse?)_serializer.Deserialize(reader);

        if (response is null || response.Header.ResultCode != "00")
        {
            _logger.LogError(
                "Onbid API error on page {PageNo}: {Code} {Msg}",
                pageNo,
                response?.Header.ResultCode,
                response?.Header.ResultMsg);

            return new KamcoPageResult { TotalCount = 0, Items = [] };
        }

        IReadOnlyList<KamcoAuctionItem> items = response.Body.Items.ItemList
            .Select(MapToDomain)
            .ToList();

        return new KamcoPageResult
        {
            TotalCount = response.Body.TotalCount,
            Items = items,
        };
    }

    private static KamcoAuctionItem MapToDomain(OnbidKamcoItem dto) => new()
    {
        PlnmNo = ParseLong(dto.PlnmNo),
        PbctNo = ParseLong(dto.PbctNo),
        PbctCdtnNo = ParseLong(dto.PbctCdtnNo),
        CltrNo = ParseLong(dto.CltrNo),
        CltrHstrNo = ParseLong(dto.CltrHstrNo),
        CltrMnmtNo = dto.CltrMnmtNo,
        CltrNm = dto.CltrNm,
        CtgrFullNm = dto.CtgrFullNm,
        LdnmAdrs = dto.LdnmAdrs,
        NmrdAdrs = dto.NmrdAdrs,
        DpslMtdCd = dto.DpslMtdCd,
        DpslMtdNm = dto.DpslMtdNm,
        MinBidPrc = ParseLong(dto.MinBidPrc),
        ApslAsesAvgAmt = ParseLong(dto.ApslAsesAvgAmt),
        FeeRate = dto.FeeRate,
        BidMtdNm = dto.BidMtdNm,
        PbctCltrStatNm = dto.PbctCltrStatNm,
        PbctBegnDtm = dto.PbctBegnDtm,
        PbctClsDtm = dto.PbctClsDtm,
        UscbdCnt = ParseInt(dto.UscbdCnt),
        IqryCnt = ParseInt(dto.IqryCnt),
        CltrImgFiles = dto.CltrImgFiles,
    };

    private static long ParseLong(string value) =>
        long.TryParse(value, out var result) ? result : 0;

    private static int ParseInt(string value) =>
        int.TryParse(value, out var result) ? result : 0;
}
