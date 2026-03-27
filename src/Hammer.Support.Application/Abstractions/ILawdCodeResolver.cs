namespace Hammer.Support.Application.Abstractions;

/// <summary>
/// Resolves a land-lot address to a 5-digit LAWD_CD (법정동 시군구 코드).
/// </summary>
public interface ILawdCodeResolver
{
    /// <summary>
    /// Extracts the 5-digit district code from a Korean land-lot address.
    /// </summary>
    /// <param name="address">Full address string (e.g. "서울특별시 강남구 역삼동 123-45").</param>
    /// <returns>The 5-digit LAWD_CD, or <c>null</c> if the address cannot be mapped.</returns>
    public string? Resolve(string address);
}
