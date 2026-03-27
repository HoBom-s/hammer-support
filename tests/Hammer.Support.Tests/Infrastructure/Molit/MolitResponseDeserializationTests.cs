using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Hammer.Support.Infrastructure.Molit.Dto;

namespace Hammer.Support.Tests.Infrastructure.Molit;

public sealed class MolitResponseDeserializationTests
{
    private static readonly XmlSerializer _serializer = new(typeof(MolitResponse));

    private static readonly XmlReaderSettings _readerSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
    };

    [Fact]
    public void Deserialize_ApartmentResponse_ReturnsCorrectFields()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <response>
                <header>
                    <resultCode>00</resultCode>
                    <resultMsg>NORMAL SERVICE.</resultMsg>
                </header>
                <body>
                    <totalCount>2</totalCount>
                    <items>
                        <item>
                            <sggCd>11680</sggCd>
                            <umdNm>역삼동</umdNm>
                            <jibun>123-4</jibun>
                            <aptNm>래미안</aptNm>
                            <dealAmount>    120,000    </dealAmount>
                            <dealYear>2026</dealYear>
                            <dealMonth>3</dealMonth>
                            <dealDay>15</dealDay>
                            <excluUseAr>84.95</excluUseAr>
                            <floor>12</floor>
                            <buildYear>2015</buildYear>
                        </item>
                        <item>
                            <sggCd>11680</sggCd>
                            <umdNm>삼성동</umdNm>
                            <jibun>456</jibun>
                            <aptNm>힐스테이트</aptNm>
                            <dealAmount>95,000</dealAmount>
                            <dealYear>2026</dealYear>
                            <dealMonth>2</dealMonth>
                            <dealDay>20</dealDay>
                            <excluUseAr>59.96</excluUseAr>
                            <floor>5</floor>
                            <buildYear>2020</buildYear>
                        </item>
                    </items>
                </body>
            </response>
            """;

        MolitResponse response = DeserializeXml(xml);

        Assert.Equal("00", response.Header.ResultCode);
        Assert.Equal("NORMAL SERVICE.", response.Header.ResultMsg);
        Assert.Equal(2, response.Body.TotalCount);
        Assert.Equal(2, response.Body.Items.ItemList.Count);

        MolitTradeItem first = response.Body.Items.ItemList[0];
        Assert.Equal("11680", first.SggCd);
        Assert.Equal("역삼동", first.UmdNm);
        Assert.Equal("123-4", first.Jibun);
        Assert.Equal("래미안", first.AptNm);
        Assert.Equal("    120,000    ", first.DealAmount);
        Assert.Equal("2026", first.DealYear);
        Assert.Equal("3", first.DealMonth);
        Assert.Equal("15", first.DealDay);
        Assert.Equal("84.95", first.ExcluUseAr);
        Assert.Equal("12", first.Floor);
        Assert.Equal("2015", first.BuildYear);
    }

    [Fact]
    public void Deserialize_LandResponse_MapsLandSpecificFields()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <response>
                <header>
                    <resultCode>00</resultCode>
                    <resultMsg>NORMAL SERVICE.</resultMsg>
                </header>
                <body>
                    <totalCount>1</totalCount>
                    <items>
                        <item>
                            <sggCd>11680</sggCd>
                            <umdNm>역삼동</umdNm>
                            <jibun>789</jibun>
                            <dealAmount>50,000</dealAmount>
                            <dealYear>2026</dealYear>
                            <dealMonth>1</dealMonth>
                            <dealDay>10</dealDay>
                            <dealArea>330.5</dealArea>
                        </item>
                    </items>
                </body>
            </response>
            """;

        MolitResponse response = DeserializeXml(xml);

        MolitTradeItem item = response.Body.Items.ItemList[0];
        Assert.Equal("330.5", item.DealArea);

        // Apartment-specific fields should be empty (not present in land response)
        Assert.Equal(string.Empty, item.AptNm);
        Assert.Equal(string.Empty, item.ExcluUseAr);
        Assert.Equal(string.Empty, item.Floor);
    }

    [Fact]
    public void Deserialize_CommercialResponse_MapsCommercialFields()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <response>
                <header>
                    <resultCode>00</resultCode>
                    <resultMsg>NORMAL SERVICE.</resultMsg>
                </header>
                <body>
                    <totalCount>1</totalCount>
                    <items>
                        <item>
                            <sggCd>11680</sggCd>
                            <umdNm>역삼동</umdNm>
                            <jibun>100</jibun>
                            <dealAmount>200,000</dealAmount>
                            <dealYear>2026</dealYear>
                            <dealMonth>3</dealMonth>
                            <dealDay>1</dealDay>
                            <buildingAr>150.3</buildingAr>
                            <floor>3</floor>
                            <buildYear>2010</buildYear>
                        </item>
                    </items>
                </body>
            </response>
            """;

        MolitResponse response = DeserializeXml(xml);

        MolitTradeItem item = response.Body.Items.ItemList[0];
        Assert.Equal("150.3", item.BuildingAr);
        Assert.Equal("3", item.Floor);
    }

    [Fact]
    public void Deserialize_ErrorResponse_ReturnsErrorCode()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <response>
                <header>
                    <resultCode>12</resultCode>
                    <resultMsg>NO OPENAPI SERVICE ERROR</resultMsg>
                </header>
                <body>
                    <totalCount>0</totalCount>
                    <items/>
                </body>
            </response>
            """;

        MolitResponse response = DeserializeXml(xml);

        Assert.Equal("12", response.Header.ResultCode);
        Assert.Equal(0, response.Body.TotalCount);
        Assert.Empty(response.Body.Items.ItemList);
    }

    [Fact]
    public void Deserialize_EmptyItems_ReturnsEmptyList()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <response>
                <header>
                    <resultCode>00</resultCode>
                    <resultMsg>NORMAL SERVICE.</resultMsg>
                </header>
                <body>
                    <totalCount>0</totalCount>
                    <items/>
                </body>
            </response>
            """;

        MolitResponse response = DeserializeXml(xml);

        Assert.Equal("00", response.Header.ResultCode);
        Assert.Equal(0, response.Body.TotalCount);
        Assert.Empty(response.Body.Items.ItemList);
    }

    private static MolitResponse DeserializeXml(string xml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var reader = XmlReader.Create(stream, _readerSettings);
        return (MolitResponse)_serializer.Deserialize(reader)!;
    }
}
