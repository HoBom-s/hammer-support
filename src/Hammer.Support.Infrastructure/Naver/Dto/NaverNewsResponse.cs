using System.Text.Json.Serialization;

namespace Hammer.Support.Infrastructure.Naver.Dto;

/// <summary>
/// Naver News Search API response.
/// </summary>
internal sealed class NaverNewsResponse
{
    /// <summary>Gets or sets the time the search result was generated.</summary>
    [JsonPropertyName("lastBuildDate")]
    public string LastBuildDate { get; set; } = string.Empty;

    /// <summary>Gets or sets the total number of search results.</summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>Gets or sets the starting index of the current page.</summary>
    [JsonPropertyName("start")]
    public int Start { get; set; }

    /// <summary>Gets or sets the number of items returned.</summary>
    [JsonPropertyName("display")]
    public int Display { get; set; }

    /// <summary>Gets or sets the list of news items.</summary>
    [JsonPropertyName("items")]
    public List<NaverNewsItemDto> Items { get; set; } = [];
}
