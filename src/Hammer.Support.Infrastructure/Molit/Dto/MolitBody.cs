using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Molit.Dto;

/// <summary>
/// Body element of the MOLIT API response.
/// </summary>
public sealed class MolitBody
{
    /// <summary>Gets or sets the total count of items.</summary>
    [XmlElement("totalCount")]
    public int TotalCount { get; set; }

    /// <summary>Gets or sets the items container.</summary>
    [XmlElement("items")]
    public MolitItems Items { get; set; } = new();
}
