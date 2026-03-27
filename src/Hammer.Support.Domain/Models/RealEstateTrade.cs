namespace Hammer.Support.Domain.Models;

/// <summary>
/// Represents a single real estate trade record from the MOLIT API.
/// </summary>
public sealed record RealEstateTrade
{
    /// <summary>Gets the district code (법정동 시군구 코드, 5자리).</summary>
    public string LawdCd { get; init; } = string.Empty;

    /// <summary>Gets the property type.</summary>
    public PropertyType PropertyType { get; init; }

    /// <summary>Gets the building/complex name (단지명). Null for land and detached houses.</summary>
    public string? BuildingName { get; init; }

    /// <summary>Gets the lot number (지번).</summary>
    public string Jibun { get; init; } = string.Empty;

    /// <summary>Gets the district name (법정동).</summary>
    public string UmdNm { get; init; } = string.Empty;

    /// <summary>Gets the deal amount in 만원 (10,000 KRW).</summary>
    public long DealAmount { get; init; }

    /// <summary>Gets the deal year.</summary>
    public int DealYear { get; init; }

    /// <summary>Gets the deal month.</summary>
    public int DealMonth { get; init; }

    /// <summary>Gets the deal day.</summary>
    public int DealDay { get; init; }

    /// <summary>Gets the area in square meters (전용면적/거래면적/대지면적).</summary>
    public decimal Area { get; init; }

    /// <summary>Gets the floor number. Null for land.</summary>
    public int? Floor { get; init; }

    /// <summary>Gets the build year. Null for land.</summary>
    public int? BuildYear { get; init; }
}
