using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Items container element of the Onbid API response.
/// </summary>
public sealed class OnbidItems
{
    /// <summary>
    /// Gets the list of item elements.
    /// </summary>
    [XmlElement("item")]
    public Collection<OnbidKamcoItem> ItemList { get; } = new();
}
