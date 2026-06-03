using System.Net;
using System.Text.RegularExpressions;
using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Models;

/// <summary>
/// Response DTO for a collected news article, with HTML stripped from text fields.
/// </summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="Query">Search keyword the article was collected under.</param>
/// <param name="Title">Article title (HTML removed).</param>
/// <param name="OriginalLink">Original article URL.</param>
/// <param name="Link">Naver-cached URL.</param>
/// <param name="Description">Article description (HTML removed).</param>
/// <param name="PubDate">Publication date.</param>
public sealed partial record NewsResponse(
    Guid Id,
    string Query,
    string Title,
    string OriginalLink,
    string Link,
    string Description,
    DateTimeOffset PubDate)
{
    /// <summary>
    /// Maps a domain entity to a response DTO, stripping HTML tags and decoding entities.
    /// </summary>
    /// <param name="article">The news article entity.</param>
    /// <returns>The mapped response DTO.</returns>
    public static NewsResponse FromEntity(NewsArticle article)
    {
        ArgumentNullException.ThrowIfNull(article);

        return new NewsResponse(
            article.Id,
            article.Query,
            StripHtml(article.Title),
            article.OriginalLink,
            article.Link,
            StripHtml(article.Description),
            article.PubDate);
    }

    private static string StripHtml(string value) =>
        WebUtility.HtmlDecode(TagRegex().Replace(value, string.Empty));

    [GeneratedRegex("<.*?>")]
    private static partial Regex TagRegex();
}
