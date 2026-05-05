namespace Hammer.Support.Application;

/// <summary>
/// Configuration settings for the hammer-internal API.
/// </summary>
public sealed class InternalApiSettings
{
    /// <summary>
    /// Gets or sets the base URI of the hammer-internal service.
    /// </summary>
    public Uri? BaseUri { get; set; }
}
