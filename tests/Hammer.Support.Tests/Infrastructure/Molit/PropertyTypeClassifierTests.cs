using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Molit;

namespace Hammer.Support.Tests.Infrastructure.Molit;

public sealed class PropertyTypeClassifierTests
{
    [Theory]
    [InlineData("주거용건물 > 아파트", PropertyType.Apartment)]
    [InlineData("아파트", PropertyType.Apartment)]
    [InlineData("주거용건물 > 단독주택", PropertyType.Detached)]
    [InlineData("주거용건물 > 다가구주택", PropertyType.Detached)]
    [InlineData("주거용건물 > 다세대주택", PropertyType.RowHouse)]
    [InlineData("주거용건물 > 연립주택", PropertyType.RowHouse)]
    [InlineData("주거용건물 > 오피스텔", PropertyType.Officetel)]
    [InlineData("상가및업무용건물 > 근린상가", PropertyType.Commercial)]
    [InlineData("업무용건물", PropertyType.Commercial)]
    [InlineData("상업용건물", PropertyType.Commercial)]
    [InlineData("토지 > 대지", PropertyType.Land)]
    [InlineData("토지 > 임야", PropertyType.Land)]
    [InlineData("토지", PropertyType.Land)]
    [InlineData("대지", PropertyType.Land)]
    public void Classify_KnownCategory_ReturnsCorrectType(string ctgrFullNm, PropertyType expected)
    {
        PropertyType result = PropertyTypeClassifier.Classify(ctgrFullNm);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("선박")]
    [InlineData("차량")]
    public void Classify_UnknownCategory_ReturnsUnknown(string ctgrFullNm)
    {
        PropertyType result = PropertyTypeClassifier.Classify(ctgrFullNm);

        Assert.Equal(PropertyType.Unknown, result);
    }
}
