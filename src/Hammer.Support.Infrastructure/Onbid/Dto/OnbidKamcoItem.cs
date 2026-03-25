using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Single KAMCO auction item as returned by the Onbid XML API.
/// </summary>
public sealed class OnbidKamcoItem
{
    /// <summary>
    /// Gets or sets the announcement number.
    /// </summary>
    [XmlElement("PLNM_NO")]
    public long PlnmNo { get; set; }

    /// <summary>
    /// Gets or sets the auction number.
    /// </summary>
    [XmlElement("PBCT_NO")]
    public long PbctNo { get; set; }

    /// <summary>
    /// Gets or sets the item number.
    /// </summary>
    [XmlElement("CLTR_NO")]
    public long CltrNo { get; set; }

    /// <summary>
    /// Gets or sets the item name.
    /// </summary>
    [XmlElement("CLTR_NM")]
    public string CltrNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category full name.
    /// </summary>
    [XmlElement("CTGR_FULL_NM")]
    public string CtgrFullNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the land lot address.
    /// </summary>
    [XmlElement("LDNM_ADRS")]
    public string LdnmAdrs { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the road name address.
    /// </summary>
    [XmlElement("NMRD_ADRS")]
    public string NmrdAdrs { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum bid price.
    /// </summary>
    [XmlElement("MIN_BID_PRC")]
    public long MinBidPrc { get; set; }

    /// <summary>
    /// Gets or sets the appraisal amount.
    /// </summary>
    [XmlElement("APSL_ASES_AVG_AMT")]
    public long ApslAsesAvgAmt { get; set; }

    /// <summary>
    /// Gets or sets the bid method name.
    /// </summary>
    [XmlElement("BID_MTD_NM")]
    public string BidMtdNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item status name.
    /// </summary>
    [XmlElement("PBCT_CLTR_STAT_NM")]
    public string PbctCltrStatNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bid start datetime.
    /// </summary>
    [XmlElement("PBCT_BEGN_DTM")]
    public string PbctBegnDtm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bid close datetime.
    /// </summary>
    [XmlElement("PBCT_CLS_DTM")]
    public string PbctClsDtm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the failed bid count.
    /// </summary>
    [XmlElement("USCBD_CNT")]
    public int UscbdCnt { get; set; }

    /// <summary>
    /// Gets or sets the inquiry count.
    /// </summary>
    [XmlElement("IQRY_CNT")]
    public int IqryCnt { get; set; }

    /// <summary>
    /// Gets or sets the item image files URL.
    /// </summary>
    [XmlElement("CLTR_IMG_FILES")]
    public string? CltrImgFiles { get; set; }
}
