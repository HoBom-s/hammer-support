using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Molit.Dto;

/// <summary>
///     Superset DTO that maps all possible XML fields across the 6 MOLIT trade APIs.
///     Fields not present in a particular API's response remain at their default (empty string).
/// </summary>
public sealed class MolitTradeItem
{
    // ── Common fields (all 6 APIs) ──

    /// <summary>Gets or sets the district code (시군구).</summary>
    [XmlElement("sggCd")]
    public string SggCd { get; set; } = string.Empty;

    /// <summary>Gets or sets the district name (법정동).</summary>
    [XmlElement("umdNm")]
    public string UmdNm { get; set; } = string.Empty;

    /// <summary>Gets or sets the lot number (지번).</summary>
    [XmlElement("jibun")]
    public string Jibun { get; set; } = string.Empty;

    /// <summary>Gets or sets the deal amount in 만원 (쉼표 포함 가능).</summary>
    [XmlElement("dealAmount")]
    public string DealAmount { get; set; } = string.Empty;

    /// <summary>Gets or sets the deal year (계약년도).</summary>
    [XmlElement("dealYear")]
    public string DealYear { get; set; } = string.Empty;

    /// <summary>Gets or sets the deal month (계약월).</summary>
    [XmlElement("dealMonth")]
    public string DealMonth { get; set; } = string.Empty;

    /// <summary>Gets or sets the deal day (계약일).</summary>
    [XmlElement("dealDay")]
    public string DealDay { get; set; } = string.Empty;

    // ── Apartment fields ──

    /// <summary>Gets or sets the apartment complex name (단지명).</summary>
    [XmlElement("aptNm")]
    public string AptNm { get; set; } = string.Empty;

    /// <summary>Gets or sets the exclusive use area (전용면적).</summary>
    [XmlElement("excluUseAr")]
    public string ExcluUseAr { get; set; } = string.Empty;

    /// <summary>Gets or sets the floor number (층).</summary>
    [XmlElement("floor")]
    public string Floor { get; set; } = string.Empty;

    /// <summary>Gets or sets the build year (건축년도).</summary>
    [XmlElement("buildYear")]
    public string BuildYear { get; set; } = string.Empty;

    // ── Detached house fields ──

    /// <summary>Gets or sets the total floor area (연면적).</summary>
    [XmlElement("totalFloorAr")]
    public string TotalFloorAr { get; set; } = string.Empty;

    /// <summary>Gets or sets the plottage area (대지면적).</summary>
    [XmlElement("plottageAr")]
    public string PlottageAr { get; set; } = string.Empty;

    // ── Row house fields ──

    /// <summary>Gets or sets the row house name (연립다세대명).</summary>
    [XmlElement("mhouseNm")]
    public string MhouseNm { get; set; } = string.Empty;

    // ── Officetel fields ──

    /// <summary>Gets or sets the officetel name (오피스텔명).</summary>
    [XmlElement("offiNm")]
    public string OffiNm { get; set; } = string.Empty;

    // ── Land fields ──

    /// <summary>Gets or sets the deal area (거래면적).</summary>
    [XmlElement("dealArea")]
    public string DealArea { get; set; } = string.Empty;

    // ── Commercial fields ──

    /// <summary>Gets or sets the building area (건물면적).</summary>
    [XmlElement("buildingAr")]
    public string BuildingAr { get; set; } = string.Empty;
}
