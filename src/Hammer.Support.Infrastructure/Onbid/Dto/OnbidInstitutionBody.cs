using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Body element of the Onbid institution API response.
/// </summary>
public sealed class OnbidInstitutionBody
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
    public OnbidInstitutionItems Items { get; set; } = new();
}
