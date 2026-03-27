using Hammer.Support.Infrastructure.Molit;

namespace Hammer.Support.Tests.Infrastructure.Molit;

public sealed class LawdCodeResolverTests
{
    private readonly LawdCodeResolver _resolver = new();

    [Theory]
    [InlineData("서울특별시 강남구 역삼동 123-45", "11680")]
    [InlineData("서울특별시 종로구 청운동", "11110")]
    [InlineData("경기도 성남시 수정구 태평동 123", "41131")]
    [InlineData("경기도 성남시 분당구 정자동", "41135")]
    [InlineData("부산광역시 해운대구 우동 456", "26350")]
    [InlineData("세종특별자치시 조치원읍 123", "36110")]
    [InlineData("제주특별자치도 제주시 이도동", "50110")]
    public void Resolve_KnownAddress_ReturnsCorrectCode(string address, string expectedCode)
    {
        var result = _resolver.Resolve(address);

        Assert.Equal(expectedCode, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null!)]
    public void Resolve_EmptyOrNull_ReturnsNull(string? address)
    {
        var result = _resolver.Resolve(address!);

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_UnknownAddress_ReturnsNull()
    {
        var result = _resolver.Resolve("알 수 없는 주소 123");

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_LongerPrefixMatchesFirst()
    {
        // "경기도 수원시 장안구"(41111) should match before any shorter prefix
        var result = _resolver.Resolve("경기도 수원시 장안구 파장동 123");

        Assert.Equal("41111", result);
    }
}
