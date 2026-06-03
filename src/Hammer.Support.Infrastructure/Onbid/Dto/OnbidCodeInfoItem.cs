using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Single code information item as returned by the Onbid XML API.
/// </summary>
public sealed class OnbidCodeInfoItem
{
    /// <summary>
    /// Gets or sets the category ID (코드 ID).
    /// </summary>
    [XmlElement("CTGR_ID")]
    public string CtgrId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category name (코드명).
    /// </summary>
    [XmlElement("CTGR_NM")]
    public string CtgrNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent category ID (상위 코드 ID).
    /// </summary>
    [XmlElement("CTGR_HIRK_ID")]
    public string CtgrHirkId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent category name (상위 코드명).
    /// </summary>
    [XmlElement("CTGR_HIRK_NM")]
    public string CtgrHirkNm { get; set; } = string.Empty;
}
