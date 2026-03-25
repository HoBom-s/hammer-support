using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Header element of the Onbid API response.
/// </summary>
public sealed class OnbidHeader
{
    /// <summary>
    /// Gets or sets the result code.
    /// </summary>
    [XmlElement("resultCode")]
    public string ResultCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the result message.
    /// </summary>
    [XmlElement("resultMsg")]
    public string ResultMsg { get; set; } = string.Empty;
}
