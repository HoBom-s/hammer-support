using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Items container element of the Onbid code information API response.
/// </summary>
public sealed class OnbidCodeInfoItems
{
    /// <summary>
    /// Gets the list of item elements.
    /// </summary>
    [XmlElement("item")]
    public Collection<OnbidCodeInfoItem> ItemList { get; } = new();
}
