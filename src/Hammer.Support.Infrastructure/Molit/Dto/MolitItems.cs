using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Molit.Dto;

/// <summary>
/// Items container element of the MOLIT API response.
/// </summary>
public sealed class MolitItems
{
    /// <summary>Gets the list of trade item elements.</summary>
    [XmlElement("item")]
    public Collection<MolitTradeItem> ItemList { get; } = new();
}
