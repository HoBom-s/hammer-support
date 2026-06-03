namespace Hammer.Support.Domain.Models;

/// <summary>
/// Represents a category code from the Onbid code information API.
/// </summary>
public sealed record OnbidCodeItem
{
    /// <summary>
    /// Gets the category ID (코드 ID).
    /// </summary>
    public string CtgrId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the category name (코드명).
    /// </summary>
    public string CtgrNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the parent category ID (상위 코드 ID).
    /// </summary>
    public string CtgrHirkId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the parent category name (상위 코드명).
    /// </summary>
    public string CtgrHirkNm { get; init; } = string.Empty;
}
