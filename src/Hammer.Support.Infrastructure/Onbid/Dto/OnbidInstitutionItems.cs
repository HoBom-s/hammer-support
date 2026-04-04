using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Items container element of the Onbid institution API response.
/// </summary>
public sealed class OnbidInstitutionItems
{
    /// <summary>
    /// Gets the list of item elements.
    /// </summary>
    [XmlElement("item")]
    public Collection<OnbidInstitutionItem> ItemList { get; } = new();
}
