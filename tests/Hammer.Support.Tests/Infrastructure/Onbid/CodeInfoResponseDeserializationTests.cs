using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Hammer.Support.Infrastructure.Onbid.Dto;

namespace Hammer.Support.Tests.Infrastructure.Onbid;

public sealed class CodeInfoResponseDeserializationTests
{
    private static readonly XmlSerializer _serializer = new(typeof(OnbidCodeInfoResponse));

    private static readonly XmlReaderSettings _readerSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
    };

    [Fact]
    public void Deserialize_ValidXml_ReturnsCorrectResponse()
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
                            <CTGR_ID>001</CTGR_ID>
                            <CTGR_NM>토지</CTGR_NM>
                            <CTGR_HIRK_ID>000</CTGR_HIRK_ID>
                            <CTGR_HIRK_NM>전체</CTGR_HIRK_NM>
                        </item>
                        <item>
                            <CTGR_ID>002</CTGR_ID>
                            <CTGR_NM>건물</CTGR_NM>
                            <CTGR_HIRK_ID>000</CTGR_HIRK_ID>
                            <CTGR_HIRK_NM>전체</CTGR_HIRK_NM>
                        </item>
                    </items>
                </body>
            </response>
            """;

        OnbidCodeInfoResponse response = DeserializeXml(xml);

        Assert.Equal("00", response.Header.ResultCode);
        Assert.Equal(2, response.Body.TotalCount);
        Assert.Equal(2, response.Body.Items.ItemList.Count);

        OnbidCodeInfoItem first = response.Body.Items.ItemList[0];
        Assert.Equal("001", first.CtgrId);
        Assert.Equal("토지", first.CtgrNm);
        Assert.Equal("000", first.CtgrHirkId);
        Assert.Equal("전체", first.CtgrHirkNm);

        OnbidCodeInfoItem second = response.Body.Items.ItemList[1];
        Assert.Equal("002", second.CtgrId);
        Assert.Equal("건물", second.CtgrNm);
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

        OnbidCodeInfoResponse response = DeserializeXml(xml);

        Assert.Equal("12", response.Header.ResultCode);
        Assert.Equal(0, response.Body.TotalCount);
        Assert.Empty(response.Body.Items.ItemList);
    }

    private static OnbidCodeInfoResponse DeserializeXml(string xml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var reader = XmlReader.Create(stream, _readerSettings);
        return (OnbidCodeInfoResponse)_serializer.Deserialize(reader)!;
    }
}
