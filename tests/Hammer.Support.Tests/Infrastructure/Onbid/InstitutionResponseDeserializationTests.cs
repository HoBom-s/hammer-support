using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Hammer.Support.Infrastructure.Onbid.Dto;

namespace Hammer.Support.Tests.Infrastructure.Onbid;

public sealed class InstitutionResponseDeserializationTests
{
    private static readonly XmlSerializer _serializer = new(typeof(OnbidInstitutionResponse));

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
                            <PLNM_KIND_CD>0001</PLNM_KIND_CD>
                            <PLNM_KIND_NM>공매</PLNM_KIND_NM>
                            <BID_DVSN_CD>01</BID_DVSN_CD>
                            <BID_DVSN_NM>전자입찰</BID_DVSN_NM>
                            <PLNM_NM>서울 강남구 공매공고</PLNM_NM>
                            <ORG_NM>한국자산관리공사</ORG_NM>
                            <PLNM_DT>20260325</PLNM_DT>
                            <ORG_PLNM_NO>2026-001</ORG_PLNM_NO>
                            <PLNM_MNMT_NO>2026-00001</PLNM_MNMT_NO>
                            <BID_MTD_CD>0001</BID_MTD_CD>
                            <BID_MTD_NM>일반경쟁</BID_MTD_NM>
                            <TOT_AMT_UNPC_DVSN_CD>01</TOT_AMT_UNPC_DVSN_CD>
                            <TOT_AMT_UNPC_DVSN_NM>총액</TOT_AMT_UNPC_DVSN_NM>
                            <DPSL_MTD_CD>0001</DPSL_MTD_CD>
                            <DPSL_MTD_NM>매각</DPSL_MTD_NM>
                            <PRPT_DVSN_CD>01</PRPT_DVSN_CD>
                            <PRPT_DVSN_NM>압류재산</PRPT_DVSN_NM>
                            <PBCT_BEGN_DTM>20260325100000</PBCT_BEGN_DTM>
                            <PBCT_CLS_DTM>20260327170000</PBCT_CLS_DTM>
                            <PBCT_EXCT_DTM>20260328100000</PBCT_EXCT_DTM>
                            <CTGR_ID>001</CTGR_ID>
                            <CTGR_FULL_NM>토지 / 대지</CTGR_FULL_NM>
                        </item>
                        <item>
                            <PLNM_NO>101</PLNM_NO>
                            <PBCT_NO>201</PBCT_NO>
                            <PLNM_KIND_CD>0002</PLNM_KIND_CD>
                            <PLNM_KIND_NM>수의</PLNM_KIND_NM>
                            <PLNM_NM>부산 해운대구 공매공고</PLNM_NM>
                            <ORG_NM>국세청</ORG_NM>
                        </item>
                    </items>
                </body>
            </response>
            """;

        OnbidInstitutionResponse response = DeserializeXml(xml);

        Assert.Equal("00", response.Header.ResultCode);
        Assert.Equal(2, response.Body.TotalCount);
        Assert.Equal(2, response.Body.Items.ItemList.Count);

        OnbidInstitutionItem first = response.Body.Items.ItemList[0];
        Assert.Equal("100", first.PlnmNo);
        Assert.Equal("200", first.PbctNo);
        Assert.Equal("0001", first.PlnmKindCd);
        Assert.Equal("공매", first.PlnmKindNm);
        Assert.Equal("서울 강남구 공매공고", first.PlnmNm);
        Assert.Equal("한국자산관리공사", first.OrgNm);
        Assert.Equal("20260325100000", first.PbctBegnDtm);
        Assert.Equal("001", first.CtgrId);
        Assert.Equal("토지 / 대지", first.CtgrFullNm);

        OnbidInstitutionItem second = response.Body.Items.ItemList[1];
        Assert.Equal("101", second.PlnmNo);
        Assert.Equal("부산 해운대구 공매공고", second.PlnmNm);
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

        OnbidInstitutionResponse response = DeserializeXml(xml);

        Assert.Equal("12", response.Header.ResultCode);
        Assert.Equal(0, response.Body.TotalCount);
        Assert.Empty(response.Body.Items.ItemList);
    }

    private static OnbidInstitutionResponse DeserializeXml(string xml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var reader = XmlReader.Create(stream, _readerSettings);
        return (OnbidInstitutionResponse)_serializer.Deserialize(reader)!;
    }
}
