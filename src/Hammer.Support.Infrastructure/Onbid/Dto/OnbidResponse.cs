using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Root XML response from the Onbid API.
/// </summary>
[XmlRoot("response")]
public sealed class OnbidResponse
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
    public OnbidBody Body { get; set; } = new();
}
