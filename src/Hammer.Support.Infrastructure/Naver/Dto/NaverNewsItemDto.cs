using System.Text.Json.Serialization;

namespace Hammer.Support.Infrastructure.Naver.Dto;

/// <summary>
/// Individual news item from the Naver News Search API.
/// </summary>
internal sealed class NaverNewsItemDto
{
    /// <summary>Gets or sets the article title (may contain HTML tags).</summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the original article URL.</summary>
    [JsonPropertyName("originallink")]
    public string OriginalLink { get; set; } = string.Empty;

    /// <summary>Gets or sets the Naver-cached URL.</summary>
    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;

    /// <summary>Gets or sets the article description (may contain HTML tags).</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the publication date string.</summary>
    [JsonPropertyName("pubDate")]
    public string PubDate { get; set; } = string.Empty;
}
