using System.Diagnostics.CodeAnalysis;
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
    /// Gets or sets the auction condition number (공매조건번호).
    /// </summary>
    [XmlElement("PBCT_CDTN_NO")]
    public long PbctCdtnNo { get; set; }

    /// <summary>
    /// Gets or sets the item number.
    /// </summary>
    [XmlElement("CLTR_NO")]
    public long CltrNo { get; set; }

    /// <summary>
    /// Gets or sets the item history number (물건이력번호).
    /// </summary>
    [XmlElement("CLTR_HSTR_NO")]
    public long CltrHstrNo { get; set; }

    /// <summary>
    /// Gets or sets the item management number (물건관리번호).
    /// </summary>
    [XmlElement("CLTR_MNMT_NO")]
    public string CltrMnmtNo { get; set; } = string.Empty;

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
    /// Gets or sets the minimum bid price rate (최저입찰가율).
    /// </summary>
    [XmlElement("FEE_RATE")]
    public string FeeRate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the disposal method code (처분방식코드: 0001=매각, 0002=임대).
    /// </summary>
    [XmlElement("DPSL_MTD_CD")]
    public string DpslMtdCd { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the disposal method name (처분방식명).
    /// </summary>
    [XmlElement("DPSL_MTD_NM")]
    public string DpslMtdNm { get; set; } = string.Empty;

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
    /// Gets or sets the item image file URLs.
    /// </summary>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "XmlSerializer requires writable collection")]
    [SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "XmlSerializer requires List<T>")]
    [XmlArray("CLTR_IMG_FILES")]
    [XmlArrayItem("CLTR_IMG_FILE")]
    public List<string> CltrImgFiles { get; set; } = [];
}
