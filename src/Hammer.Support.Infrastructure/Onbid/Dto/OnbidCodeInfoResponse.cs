using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Root XML response from the Onbid code information API.
/// </summary>
[XmlRoot("response")]
public sealed class OnbidCodeInfoResponse
{
    /// <summary>
    /// Gets or sets the response header.
    /// </summary>
    [XmlElement("header")]
    public OnbidHeader Header { get; set; } = new();

    /// <summary>
    /// Gets or sets the response body.
    /// </summary>
    [XmlElement("body")]
    public OnbidCodeInfoBody Body { get; set; } = new();
}
