using System.Diagnostics.CodeAnalysis;

namespace Hammer.Support.Infrastructure.Molit;

/// <summary>
/// Configuration options for the MOLIT (국토교통부) real estate trade API.
/// </summary>
public sealed class MolitOptions
{
    /// <summary>Configuration section name in appsettings.</summary>
    public const string SectionName = "Molit";

    /// <summary>Gets or sets the API service key for authentication.</summary>
    public string ServiceKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the base URL for the MOLIT API.</summary>
    [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Configuration binding requires string")]
    public string BaseUrl { get; set; } = "https://apis.data.go.kr/1613000";

    /// <summary>Gets or sets the number of items per page.</summary>
    public int PageSize { get; set; } = 100;

    /// <summary>Gets or sets the number of months to look back for trade data.</summary>
    public int MonthsToCollect { get; set; } = 3;
}
