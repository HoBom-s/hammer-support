using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Json;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Naver.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Naver;

/// <summary>
/// HTTP client for the Naver News Search API.
/// </summary>
[SuppressMessage("Design", "S1075:Refactor your code not to use hardcoded absolute paths or URIs", Justification = "Fixed API endpoint")]
public sealed class NaverNewsApiClient : INaverNewsApiClient
{
    private const string BaseUrl = "https://openapi.naver.com/v1/search/news.json";

    private readonly HttpClient _httpClient;
    private readonly NaverOptions _options;
    private readonly ILogger<NaverNewsApiClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NaverNewsApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="options">Naver API configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public NaverNewsApiClient(
        HttpClient httpClient,
        IOptions<NaverOptions> options,
        ILogger<NaverNewsApiClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NewsArticle>> SearchAsync(
        string query,
        int display = 100,
        CancellationToken cancellationToken = default)
    {
        var uri = $"{BaseUrl}?query={Uri.EscapeDataString(query)}&display={display}&sort=date";

        using HttpRequestMessage request = new(HttpMethod.Get, uri);
        request.Headers.Add("X-Naver-Client-Id", _options.ClientId);
        request.Headers.Add("X-Naver-Client-Secret", _options.ClientSecret);

        using HttpResponseMessage httpResponse = await _httpClient.SendAsync(request, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Naver News API error: {StatusCode} {Body}", httpResponse.StatusCode, body);
            httpResponse.EnsureSuccessStatusCode();
        }

        NaverNewsResponse? response = await httpResponse.Content.ReadFromJsonAsync<NaverNewsResponse>(cancellationToken);

        if (response is null)
            return [];

        return response.Items.Select(i => new NewsArticle
        {
            Title = i.Title,
            OriginalLink = i.OriginalLink,
            Link = i.Link,
            Description = i.Description,
            PubDate = ParsePubDate(i.PubDate),
        }).ToList();
    }

    private static DateTimeOffset ParsePubDate(string pubDate)
    {
        // Naver API returns RFC 1123 format: "Mon, 02 Jun 2025 09:00:00 +0900"
        if (DateTimeOffset.TryParseExact(
                pubDate,
                "ddd, dd MMM yyyy HH:mm:ss zzz",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset parsed))
            return parsed;

        if (DateTimeOffset.TryParse(
                pubDate,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset fallback))
            return fallback;

        return DateTimeOffset.UtcNow;
    }
}
