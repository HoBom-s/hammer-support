using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Onbid.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Onbid.CodeInfo;

/// <summary>
/// HTTP client for fetching category codes from the Onbid code information API.
/// </summary>
public sealed class CodeInfoApiClient : ICodeInfoApiClient
{
    private static readonly XmlSerializer _serializer = new(typeof(OnbidCodeInfoResponse));

    private static readonly XmlReaderSettings _readerSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
    };

    private readonly HttpClient _httpClient;
    private readonly OnbidOptions _options;
    private readonly ILogger<CodeInfoApiClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeInfoApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client instance.</param>
    /// <param name="options">Onbid configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public CodeInfoApiClient(HttpClient httpClient, IOptions<OnbidOptions> options, ILogger<CodeInfoApiClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Runs once per page in daily batch")]
    public async Task<CodeInfoPageResult> FetchPageAsync(int pageNo, int numOfRows, CancellationToken cancellationToken = default)
    {
        Uri uri = new($"{_options.CodeBaseUrl}/getOnbidTopCodeInfo"
            + $"?ServiceKey={_options.ServiceKey}"
            + $"&numOfRows={numOfRows}"
            + $"&pageNo={pageNo}");

        _logger.LogInformation("Fetching code info page {PageNo} ({NumOfRows} rows)", pageNo, numOfRows);

        using Stream stream = await _httpClient.GetStreamAsync(uri, cancellationToken);
        using var reader = XmlReader.Create(stream, _readerSettings);
        var response = (OnbidCodeInfoResponse?)_serializer.Deserialize(reader);

        if (response is null || response.Header.ResultCode != "00")
        {
            _logger.LogError(
                "Onbid code info API error on page {PageNo}: {Code} {Msg}",
                pageNo,
                response?.Header.ResultCode,
                response?.Header.ResultMsg);

            return new CodeInfoPageResult { TotalCount = 0, Items = [] };
        }

        IReadOnlyList<OnbidCodeItem> items = response.Body.Items.ItemList
            .Select(MapToDomain)
            .ToList();

        return new CodeInfoPageResult
        {
            TotalCount = response.Body.TotalCount,
            Items = items,
        };
    }

    private static OnbidCodeItem MapToDomain(OnbidCodeInfoItem dto) => new()
    {
        CtgrId = dto.CtgrId,
        CtgrNm = dto.CtgrNm,
        CtgrHirkId = dto.CtgrHirkId,
        CtgrHirkNm = dto.CtgrHirkNm,
    };
}
