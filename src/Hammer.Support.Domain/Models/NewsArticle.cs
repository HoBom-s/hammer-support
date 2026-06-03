namespace Hammer.Support.Domain.Models;

/// <summary>
/// A collected news article from the Naver News API.
/// </summary>
public sealed class NewsArticle
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the search keyword used to collect this article.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Gets or sets the article title (may contain HTML tags).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the original article URL.</summary>
    public string OriginalLink { get; set; } = string.Empty;

    /// <summary>Gets or sets the Naver-cached URL.</summary>
    public string Link { get; set; } = string.Empty;

    /// <summary>Gets or sets the article description (may contain HTML tags).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the publication date.</summary>
    public DateTimeOffset PubDate { get; set; }

    /// <summary>Gets or sets the timestamp when this article was collected.</summary>
    public DateTimeOffset CollectedAt { get; set; }
}
