using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Molit.Dto;

/// <summary>
/// Root XML response from the MOLIT real estate trade API.
/// </summary>
[XmlRoot("response")]
public sealed class MolitResponse
{
    /// <summary>Gets or sets the response header.</summary>
    [XmlElement("header")]
    public MolitHeader Header { get; set; } = new();

    /// <summary>Gets or sets the response body.</summary>
    [XmlElement("body")]
    public MolitBody Body { get; set; } = new();
}
