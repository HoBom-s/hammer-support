using Hammer.Support.Domain.Models;

namespace Hammer.Support.Infrastructure.Molit;

/// <summary>
///     Classifies KAMCO <c>CtgrFullNm</c> (용도명) into a <see cref="PropertyType" />
///     to determine which MOLIT real estate trade API to call.
/// </summary>
internal static class PropertyTypeClassifier
{
    /// <summary>
    ///     Maps a KAMCO category name to a property type.
    /// </summary>
    /// <param name="ctgrFullNm">The full category name from the KAMCO auction item.</param>
    /// <returns>The classified property type, or <see cref="PropertyType.Unknown" /> if unrecognized.</returns>
    public static PropertyType Classify(string ctgrFullNm)
    {
        if (string.IsNullOrWhiteSpace(ctgrFullNm))
            return PropertyType.Unknown;

        // Order matters: more specific keywords first.
        if (ctgrFullNm.Contains("아파트", StringComparison.Ordinal))
            return PropertyType.Apartment;

        if (ctgrFullNm.Contains("오피스텔", StringComparison.Ordinal))
            return PropertyType.Officetel;

        if (ctgrFullNm.Contains("다세대", StringComparison.Ordinal)
            || ctgrFullNm.Contains("연립", StringComparison.Ordinal))
            return PropertyType.RowHouse;

        if (ctgrFullNm.Contains("단독", StringComparison.Ordinal)
            || ctgrFullNm.Contains("다가구", StringComparison.Ordinal))
            return PropertyType.Detached;

        if (ctgrFullNm.Contains("상가", StringComparison.Ordinal)
            || ctgrFullNm.Contains("업무용", StringComparison.Ordinal)
            || ctgrFullNm.Contains("상업", StringComparison.Ordinal))
            return PropertyType.Commercial;

        if (ctgrFullNm.Contains("토지", StringComparison.Ordinal)
            || ctgrFullNm.Contains("대지", StringComparison.Ordinal)
            || ctgrFullNm.Contains("임야", StringComparison.Ordinal)
            || ctgrFullNm.Contains('전', StringComparison.Ordinal)
            || ctgrFullNm.Contains('답', StringComparison.Ordinal))
            return PropertyType.Land;

        return PropertyType.Unknown;
    }
}
