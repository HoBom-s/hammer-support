namespace Hammer.Support.Domain.Models;

/// <summary>
/// Real estate property type used to determine which MOLIT API to call.
/// </summary>
public enum PropertyType
{
    /// <summary>Unclassified or unsupported property type.</summary>
    Unknown = 0,

    /// <summary>아파트.</summary>
    Apartment,

    /// <summary>단독/다가구 주택.</summary>
    Detached,

    /// <summary>연립/다세대 주택.</summary>
    RowHouse,

    /// <summary>오피스텔.</summary>
    Officetel,

    /// <summary>토지.</summary>
    Land,

    /// <summary>상업/업무용 건물.</summary>
    Commercial,
}
