using System.Diagnostics.CodeAnalysis;

namespace Hammer.Support.Infrastructure.Onbid;

/// <summary>
/// Configuration options for the Onbid public API.
/// </summary>
public sealed class OnbidOptions
{
    /// <summary>
    /// Configuration section name in appsettings.
    /// </summary>
    public const string SectionName = "Onbid";

    /// <summary>
    /// Gets or sets the API service key for authentication.
    /// </summary>
    public string ServiceKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for KAMCO public auction API.
    /// </summary>
    [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Configuration binding requires string")]
    public string KamcoBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for institutional public auction API.
    /// </summary>
    [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Configuration binding requires string")]
    public string InstitutionBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for code information API.
    /// </summary>
    [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Configuration binding requires string")]
    public string CodeBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the hour (0-23, KST) at which the daily collection job runs.
    /// </summary>
    public int CollectionHour { get; set; } = 9;
}
