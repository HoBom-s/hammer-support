using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Body element of the Onbid API response.
/// </summary>
public sealed class OnbidBody
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
    public OnbidItems Items { get; set; } = new();
}
