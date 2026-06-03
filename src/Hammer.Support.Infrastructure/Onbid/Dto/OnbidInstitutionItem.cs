using System.Xml.Serialization;

namespace Hammer.Support.Infrastructure.Onbid.Dto;

/// <summary>
/// Single institution public sale announcement as returned by the Onbid XML API.
/// All fields are kept as strings because the Onbid API may return
/// non-numeric placeholders such as "-" for missing values.
/// </summary>
public sealed class OnbidInstitutionItem
{
    /// <summary>
    /// Gets or sets the announcement number (공고번호).
    /// </summary>
    [XmlElement("PLNM_NO")]
    public string PlnmNo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the auction number (공매번호).
    /// </summary>
    [XmlElement("PBCT_NO")]
    public string PbctNo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the announcement type code (공고종류코드).
    /// </summary>
    [XmlElement("PLNM_KIND_CD")]
    public string PlnmKindCd { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the announcement type name (공고종류).
    /// </summary>
    [XmlElement("PLNM_KIND_NM")]
    public string PlnmKindNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bid form code (입찰형태코드).
    /// </summary>
    [XmlElement("BID_DVSN_CD")]
    public string BidDvsnCd { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bid form name (입찰형태).
    /// </summary>
    [XmlElement("BID_DVSN_NM")]
    public string BidDvsnNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the announcement name (공고명).
    /// </summary>
    [XmlElement("PLNM_NM")]
    public string PlnmNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the announcing organization name (공고기관명).
    /// </summary>
    [XmlElement("ORG_NM")]
    public string OrgNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the announcement date (공고일자, YYYYMMDD).
    /// </summary>
    [XmlElement("PLNM_DT")]
    public string PlnmDt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the organization announcement number (기관공고번호).
    /// </summary>
    [XmlElement("ORG_PLNM_NO")]
    public string OrgPlnmNo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the announcement management number (공고관리번호).
    /// </summary>
    [XmlElement("PLNM_MNMT_NO")]
    public string PlnmMnmtNo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bid method code (입찰방식코드).
    /// </summary>
    [XmlElement("BID_MTD_CD")]
    public string BidMtdCd { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bid method name (입찰방식).
    /// </summary>
    [XmlElement("BID_MTD_NM")]
    public string BidMtdNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total/unit price division code (총액단가구분코드).
    /// </summary>
    [XmlElement("TOT_AMT_UNPC_DVSN_CD")]
    public string TotAmtUnpcDvsnCd { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total/unit price division name (총액단가구분).
    /// </summary>
    [XmlElement("TOT_AMT_UNPC_DVSN_NM")]
    public string TotAmtUnpcDvsnNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the disposal method code (처분방식코드).
    /// </summary>
    [XmlElement("DPSL_MTD_CD")]
    public string DpslMtdCd { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the disposal method name (처분방식).
    /// </summary>
    [XmlElement("DPSL_MTD_NM")]
    public string DpslMtdNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the property division code (재산구분코드).
    /// </summary>
    [XmlElement("PRPT_DVSN_CD")]
    public string PrptDvsnCd { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the property division name (재산구분).
    /// </summary>
    [XmlElement("PRPT_DVSN_NM")]
    public string PrptDvsnNm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bid start datetime (입찰시작일시, YYYYMMDDHH24MISS).
    /// </summary>
    [XmlElement("PBCT_BEGN_DTM")]
    public string PbctBegnDtm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bid close datetime (입찰마감일시, YYYYMMDDHH24MISS).
    /// </summary>
    [XmlElement("PBCT_CLS_DTM")]
    public string PbctClsDtm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bid opening datetime (개찰일시, YYYYMMDDHH24MISS).
    /// </summary>
    [XmlElement("PBCT_EXCT_DTM")]
    public string PbctExctDtm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category ID (용도코드).
    /// </summary>
    [XmlElement("CTGR_ID")]
    public string CtgrId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full category name (용도).
    /// </summary>
    [XmlElement("CTGR_FULL_NM")]
    public string CtgrFullNm { get; set; } = string.Empty;
}
