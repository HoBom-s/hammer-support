namespace Hammer.Support.Domain.Models;

/// <summary>
/// Represents a KAMCO public auction item from the Onbid API.
/// </summary>
public sealed record KamcoAuctionItem
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
    /// Gets the auction condition number (공매조건번호).
    /// </summary>
    public long PbctCdtnNo { get; init; }

    /// <summary>
    /// Gets the item number (물건번호).
    /// </summary>
    public long CltrNo { get; init; }

    /// <summary>
    /// Gets the item history number (물건이력번호).
    /// </summary>
    public long CltrHstrNo { get; init; }

    /// <summary>
    /// Gets the item management number (물건관리번호).
    /// </summary>
    public string CltrMnmtNo { get; init; } = string.Empty;

    /// <summary>
    /// Gets the item name (물건명).
    /// </summary>
    public string CltrNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the full category name (용도명).
    /// </summary>
    public string CtgrFullNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the land lot address (지번주소).
    /// </summary>
    public string LdnmAdrs { get; init; } = string.Empty;

    /// <summary>
    /// Gets the road name address (도로명주소).
    /// </summary>
    public string NmrdAdrs { get; init; } = string.Empty;

    /// <summary>
    /// Gets the minimum bid price (최저입찰가).
    /// </summary>
    public long MinBidPrc { get; init; }

    /// <summary>
    /// Gets the average appraisal amount (감정가).
    /// </summary>
    public long ApslAsesAvgAmt { get; init; }

    /// <summary>
    /// Gets the minimum bid price rate (최저입찰가율).
    /// </summary>
    public string FeeRate { get; init; } = string.Empty;

    /// <summary>
    /// Gets the disposal method code (처분방식코드: 0001=매각, 0002=임대).
    /// </summary>
    public string DpslMtdCd { get; init; } = string.Empty;

    /// <summary>
    /// Gets the disposal method name (처분방식명).
    /// </summary>
    public string DpslMtdNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bid method name (입찰방식명).
    /// </summary>
    public string BidMtdNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the item status name (물건상태).
    /// </summary>
    public string PbctCltrStatNm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bid start datetime in YYYYMMDDHH24MISS format (입찰시작일시).
    /// </summary>
    public string PbctBegnDtm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the bid close datetime in YYYYMMDDHH24MISS format (입찰마감일시).
    /// </summary>
    public string PbctClsDtm { get; init; } = string.Empty;

    /// <summary>
    /// Gets the failed bid count (유찰횟수).
    /// </summary>
    public int UscbdCnt { get; init; }

    /// <summary>
    /// Gets the inquiry count (조회수).
    /// </summary>
    public int IqryCnt { get; init; }

    /// <summary>
    /// Gets the item image files URL (물건이미지).
    /// </summary>
    public string? CltrImgFiles { get; init; }
}
