using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Hammer.Support.Infrastructure.Onbid.Dto;

namespace Hammer.Support.Tests.Infrastructure.Onbid;

public sealed class OnbidResponseDeserializationTests
{
    private static readonly XmlSerializer _serializer = new(typeof(OnbidResponse));

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
                            <PLNM_NO>100</PLNM_NO>
                            <PBCT_NO>200</PBCT_NO>
                            <PBCT_CDTN_NO>400</PBCT_CDTN_NO>
                            <CLTR_NO>300</CLTR_NO>
                            <CLTR_HSTR_NO>500</CLTR_HSTR_NO>
                            <CLTR_MNMT_NO>2025-00001-001</CLTR_MNMT_NO>
                            <CLTR_NM>서울 강남구 역삼동</CLTR_NM>
                            <CTGR_FULL_NM>토지 / 대지</CTGR_FULL_NM>
                            <LDNM_ADRS>서울 강남구 역삼동 123</LDNM_ADRS>
                            <NMRD_ADRS>서울 강남구 테헤란로 1</NMRD_ADRS>
                            <DPSL_MTD_CD>0001</DPSL_MTD_CD>
                            <DPSL_MTD_NM>매각</DPSL_MTD_NM>
                            <MIN_BID_PRC>500000000</MIN_BID_PRC>
                            <APSL_ASES_AVG_AMT>700000000</APSL_ASES_AVG_AMT>
                            <FEE_RATE>(100%)</FEE_RATE>
                            <BID_MTD_NM>일반경쟁</BID_MTD_NM>
                            <PBCT_CLTR_STAT_NM>입찰진행중</PBCT_CLTR_STAT_NM>
                            <PBCT_BEGN_DTM>20260325100000</PBCT_BEGN_DTM>
                            <PBCT_CLS_DTM>20260327170000</PBCT_CLS_DTM>
                            <USCBD_CNT>3</USCBD_CNT>
                            <IQRY_CNT>42</IQRY_CNT>
                        </item>
                        <item>
                            <PLNM_NO>101</PLNM_NO>
                            <PBCT_NO>201</PBCT_NO>
                            <PBCT_CDTN_NO>401</PBCT_CDTN_NO>
                            <CLTR_NO>301</CLTR_NO>
                            <CLTR_HSTR_NO>501</CLTR_HSTR_NO>
                            <CLTR_MNMT_NO>2025-00002-001</CLTR_MNMT_NO>
                            <CLTR_NM>부산 해운대구</CLTR_NM>
                            <CTGR_FULL_NM>건물 / 아파트</CTGR_FULL_NM>
                            <LDNM_ADRS>부산 해운대구 456</LDNM_ADRS>
                            <NMRD_ADRS>부산 해운대로 2</NMRD_ADRS>
                            <DPSL_MTD_CD>0001</DPSL_MTD_CD>
                            <DPSL_MTD_NM>매각</DPSL_MTD_NM>
                            <MIN_BID_PRC>300000000</MIN_BID_PRC>
                            <APSL_ASES_AVG_AMT>400000000</APSL_ASES_AVG_AMT>
                            <FEE_RATE>(90%)</FEE_RATE>
                            <BID_MTD_NM>일반경쟁</BID_MTD_NM>
                            <PBCT_CLTR_STAT_NM>입찰준비중</PBCT_CLTR_STAT_NM>
                            <PBCT_BEGN_DTM>20260401100000</PBCT_BEGN_DTM>
                            <PBCT_CLS_DTM>20260403170000</PBCT_CLS_DTM>
                            <USCBD_CNT>0</USCBD_CNT>
                            <IQRY_CNT>10</IQRY_CNT>
                        </item>
                    </items>
                </body>
            </response>
            """;

        OnbidResponse response = DeserializeXml(xml);

        Assert.Equal("00", response.Header.ResultCode);
        Assert.Equal(2, response.Body.TotalCount);
        Assert.Equal(2, response.Body.Items.ItemList.Count);

        OnbidKamcoItem first = response.Body.Items.ItemList[0];
        Assert.Equal("100", first.PlnmNo);
        Assert.Equal("200", first.PbctNo);
        Assert.Equal("300", first.CltrNo);
        Assert.Equal("서울 강남구 역삼동", first.CltrNm);
        Assert.Equal("500000000", first.MinBidPrc);
        Assert.Equal("700000000", first.ApslAsesAvgAmt);
        Assert.Equal("20260325100000", first.PbctBegnDtm);
        Assert.Equal("3", first.UscbdCnt);
        Assert.Equal("42", first.IqryCnt);
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

        OnbidResponse response = DeserializeXml(xml);

        Assert.Equal("12", response.Header.ResultCode);
        Assert.Equal(0, response.Body.TotalCount);
        Assert.Empty(response.Body.Items.ItemList);
    }

    private static OnbidResponse DeserializeXml(string xml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var reader = XmlReader.Create(stream, _readerSettings);
        return (OnbidResponse)_serializer.Deserialize(reader)!;
    }
}
