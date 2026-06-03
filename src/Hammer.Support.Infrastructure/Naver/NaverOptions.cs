namespace Hammer.Support.Infrastructure.Naver;

/// <summary>
/// Configuration options for the Naver Open API.
/// </summary>
public sealed class NaverOptions
{
    /// <summary>Configuration section name in appsettings.</summary>
    public const string SectionName = "Naver";

    /// <summary>Gets or sets the Naver API client ID.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Gets or sets the Naver API client secret.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Gets or sets the search keywords for batch collection.</summary>
    public IReadOnlyList<string> Keywords { get; set; } = ["경매", "부동산"];

    /// <summary>Gets or sets the number of items to fetch per keyword (max 100).</summary>
    public int Display { get; set; } = 100;

    /// <summary>Gets or sets the collection interval in minutes.</summary>
    public int CollectionIntervalMinutes { get; set; } = 60;
}
