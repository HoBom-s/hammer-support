using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;

namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Client for fetching real estate trade data from the MOLIT (국토교통부) API.
/// </summary>
public interface IRealEstateApiClient
{
    /// <summary>
    /// Fetches a single page of real estate trades for the given property type, district, and deal month.
    /// </summary>
    /// <param name="propertyType">The property type that determines which API endpoint to call.</param>
    /// <param name="lawdCd">The 5-digit district code (법정동 시군구 코드).</param>
    /// <param name="dealYmd">The deal year-month in YYYYMM format.</param>
    /// <param name="pageNo">The 1-based page number.</param>
    /// <param name="numOfRows">Number of rows per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The fetched page result containing items and total count.</returns>
    public Task<RealEstatePageResult> FetchPageAsync(
        PropertyType propertyType,
        string lawdCd,
        string dealYmd,
        int pageNo,
        int numOfRows,
        CancellationToken cancellationToken = default);
}
