namespace Hammer.Support.Domain.Models;

/// <summary>
/// Represents an institution public sale announcement from the Onbid API.
/// </summary>
public sealed record InstitutionAuctionItem
{
    /// <summary>
    /// Gets the announcement number (공고번호).
    /// </summary>
    public long PlnmNo { get; init; }

    /// <summary>
    /// Gets the auction number (공매번호).
    /// </summary>
    public long PbctNo { get; init; }

    /// <summary>
    /// Gets the announcement type code (공고종류코드).
    /// </summary>
    public string PlnmKindCd { get; init; } = string.Empty;

    /// <summary>
    /// Gets the announcement type name (공고종류).
    /// </summary>
    public string PlnmKindNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bid form code (입찰형태코드).
    /// </summary>
    public string BidDvsnCd { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bid form name (입찰형태).
    /// </summary>
    public string BidDvsnNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the announcement name (공고명).
    /// </summary>
    public string PlnmNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the announcing organization name (공고기관명).
    /// </summary>
    public string OrgNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the announcement date in YYYYMMDD format (공고일자).
    /// </summary>
    public string PlnmDt { get; init; } = string.Empty;

    /// <summary>
    /// Gets the organization announcement number (기관공고번호).
    /// </summary>
    public string OrgPlnmNo { get; init; } = string.Empty;

    /// <summary>
    /// Gets the announcement management number (공고관리번호).
    /// </summary>
    public string PlnmMnmtNo { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bid method code (입찰방식코드).
    /// </summary>
    public string BidMtdCd { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bid method name (입찰방식).
    /// </summary>
    public string BidMtdNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the total/unit price division code (총액단가구분코드).
    /// </summary>
    public string TotAmtUnpcDvsnCd { get; init; } = string.Empty;

    /// <summary>
    /// Gets the total/unit price division name (총액단가구분).
    /// </summary>
    public string TotAmtUnpcDvsnNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the disposal method code (처분방식코드).
    /// </summary>
    public string DpslMtdCd { get; init; } = string.Empty;

    /// <summary>
    /// Gets the disposal method name (처분방식).
    /// </summary>
    public string DpslMtdNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the property division code (재산구분코드).
    /// </summary>
    public string PrptDvsnCd { get; init; } = string.Empty;

    /// <summary>
    /// Gets the property division name (재산구분).
    /// </summary>
    public string PrptDvsnNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bid start datetime in YYYYMMDDHH24MISS format (입찰시작일시).
    /// </summary>
    public string PbctBegnDtm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bid close datetime in YYYYMMDDHH24MISS format (입찰마감일시).
    /// </summary>
    public string PbctClsDtm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bid opening datetime in YYYYMMDDHH24MISS format (개찰일시).
    /// </summary>
    public string PbctExctDtm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the category ID (용도코드).
    /// </summary>
    public string CtgrId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the full category name (용도).
    /// </summary>
    public string CtgrFullNm { get; init; } = string.Empty;
}
