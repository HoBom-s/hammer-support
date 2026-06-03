using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Molit.Dto;

/// <summary>
/// Header element of the MOLIT API response.
/// </summary>
public sealed class MolitHeader
{
    /// <summary>Gets or sets the result code ("00" = success).</summary>
    [XmlElement("resultCode")]
    public string ResultCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the result message.</summary>
    [XmlElement("resultMsg")]
    public string ResultMsg { get; set; } = string.Empty;
}
