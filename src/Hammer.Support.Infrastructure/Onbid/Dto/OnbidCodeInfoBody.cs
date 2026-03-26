using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Body element of the Onbid code information API response.
/// </summary>
public sealed class OnbidCodeInfoBody
{
    /// <summary>
    /// Gets or sets the total count of items.
    /// </summary>
    [XmlElement("totalCount")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the items container.
    /// </summary>
    [XmlElement("items")]
    public OnbidCodeInfoItems Items { get; set; } = new();
}
