using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Onbid.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Onbid.Institution;

/// <summary>
/// HTTP client for fetching institution public sale announcements from the Onbid API.
/// </summary>
public sealed class InstitutionApiClient : IInstitutionApiClient
{
    private static readonly XmlSerializer _serializer = new(typeof(OnbidInstitutionResponse));

    private static readonly XmlReaderSettings _readerSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
    };

    private readonly HttpClient _httpClient;
    private readonly OnbidOptions _options;
    private readonly ILogger<InstitutionApiClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstitutionApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client instance.</param>
    /// <param name="options">Onbid configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public InstitutionApiClient(HttpClient httpClient, IOptions<OnbidOptions> options, ILogger<InstitutionApiClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Runs once per page in daily batch")]
    public async Task<InstitutionPageResult> FetchPageAsync(int pageNo, int numOfRows, CancellationToken cancellationToken = default)
    {
        Uri uri = new($"{_options.InstitutionBaseUrl}/getPublicSaleAnnouncement"
            + $"?ServiceKey={_options.ServiceKey}"
            + $"&numOfRows={numOfRows}"
            + $"&pageNo={pageNo}");

        _logger.LogInformation("Fetching institution auction page {PageNo} ({NumOfRows} rows)", pageNo, numOfRows);

        using Stream stream = await _httpClient.GetStreamAsync(uri, cancellationToken);
        using var reader = XmlReader.Create(stream, _readerSettings);
        var response = (OnbidInstitutionResponse?)_serializer.Deserialize(reader);

        if (response is null || response.Header.ResultCode != "00")
        {
            _logger.LogError(
                "Onbid institution API error on page {PageNo}: {Code} {Msg}",
                pageNo,
                response?.Header.ResultCode,
                response?.Header.ResultMsg);

            return new InstitutionPageResult { TotalCount = 0, Items = [] };
        }

        IReadOnlyList<InstitutionAuctionItem> items = response.Body.Items.ItemList
            .Select(MapToDomain)
            .ToList();

        return new InstitutionPageResult
        {
            TotalCount = response.Body.TotalCount,
            Items = items,
        };
    }

    private static InstitutionAuctionItem MapToDomain(OnbidInstitutionItem dto) => new()
    {
        PlnmNo = ParseLong(dto.PlnmNo),
        PbctNo = ParseLong(dto.PbctNo),
        PlnmKindCd = dto.PlnmKindCd,
        PlnmKindNm = dto.PlnmKindNm,
        BidDvsnCd = dto.BidDvsnCd,
        BidDvsnNm = dto.BidDvsnNm,
        PlnmNm = dto.PlnmNm,
        OrgNm = dto.OrgNm,
        PlnmDt = dto.PlnmDt,
        OrgPlnmNo = dto.OrgPlnmNo,
        PlnmMnmtNo = dto.PlnmMnmtNo,
        BidMtdCd = dto.BidMtdCd,
        BidMtdNm = dto.BidMtdNm,
        TotAmtUnpcDvsnCd = dto.TotAmtUnpcDvsnCd,
        TotAmtUnpcDvsnNm = dto.TotAmtUnpcDvsnNm,
        DpslMtdCd = dto.DpslMtdCd,
        DpslMtdNm = dto.DpslMtdNm,
        PrptDvsnCd = dto.PrptDvsnCd,
        PrptDvsnNm = dto.PrptDvsnNm,
        PbctBegnDtm = dto.PbctBegnDtm,
        PbctClsDtm = dto.PbctClsDtm,
        PbctExctDtm = dto.PbctExctDtm,
        CtgrId = dto.CtgrId,
        CtgrFullNm = dto.CtgrFullNm,
    };

    private static long ParseLong(string value) =>
        long.TryParse(value, out var result) ? result : 0;
}
