using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Onbid;
using Hammer.Support.Infrastructure.Onbid.Institution;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Tests.Infrastructure.Onbid;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Test lifecycle managed by xUnit")]
public sealed class InstitutionApiClientTests
{
    private static readonly OnbidOptions _defaultOptions = new() { ServiceKey = "test-key", InstitutionBaseUrl = "http://localhost", PageSize = 100 };

    [Fact]
    public async Task FetchPageAsync_ValidResponse_ReturnsMappedItems()
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
                                           <PLNM_NO>100</PLNM_NO>
                                           <PBCT_NO>200</PBCT_NO>
                                           <PLNM_KIND_CD>0001</PLNM_KIND_CD>
                                           <PLNM_KIND_NM>공매</PLNM_KIND_NM>
                                           <BID_DVSN_CD>01</BID_DVSN_CD>
                                           <BID_DVSN_NM>전자입찰</BID_DVSN_NM>
                                           <PLNM_NM>서울 강남구 물건 공매공고</PLNM_NM>
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
                                   </items>
                               </body>
                           </response>
                           """;

        InstitutionApiClient client = CreateClient(xml);

        InstitutionPageResult result = await client.FetchPageAsync(1, 100);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);

        InstitutionAuctionItem item = result.Items[0];
        Assert.Equal(100, item.PlnmNo);
        Assert.Equal(200, item.PbctNo);
        Assert.Equal("0001", item.PlnmKindCd);
        Assert.Equal("공매", item.PlnmKindNm);
        Assert.Equal("서울 강남구 물건 공매공고", item.PlnmNm);
        Assert.Equal("한국자산관리공사", item.OrgNm);
        Assert.Equal("20260325", item.PlnmDt);
        Assert.Equal("일반경쟁", item.BidMtdNm);
        Assert.Equal("매각", item.DpslMtdNm);
        Assert.Equal("압류재산", item.PrptDvsnNm);
        Assert.Equal("20260325100000", item.PbctBegnDtm);
        Assert.Equal("20260327170000", item.PbctClsDtm);
        Assert.Equal("20260328100000", item.PbctExctDtm);
        Assert.Equal("001", item.CtgrId);
        Assert.Equal("토지 / 대지", item.CtgrFullNm);
    }

    [Fact]
    public async Task FetchPageAsync_NonNumericPlnmNo_ParsesAsZero()
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
                                           <PLNM_NO>-</PLNM_NO>
                                           <PBCT_NO>-</PBCT_NO>
                                       </item>
                                   </items>
                               </body>
                           </response>
                           """;

        InstitutionApiClient client = CreateClient(xml);

        InstitutionPageResult result = await client.FetchPageAsync(1, 100);

        Assert.Equal(0, result.Items[0].PlnmNo);
        Assert.Equal(0, result.Items[0].PbctNo);
    }

    [Fact]
    public async Task FetchPageAsync_ErrorResponse_ReturnsEmptyResult()
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

        InstitutionApiClient client = CreateClient(xml);

        InstitutionPageResult result = await client.FetchPageAsync(1, 100);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    private static InstitutionApiClient CreateClient(string xmlResponse)
    {
        FakeHttpMessageHandler handler = new(xmlResponse);
        HttpClient httpClient = new(handler);
        IOptions<OnbidOptions> options = Options.Create(_defaultOptions);

        return new InstitutionApiClient(httpClient, options, NullLogger<InstitutionApiClient>.Instance);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;

        public FakeHttpMessageHandler(string responseContent)
        {
            _responseContent = responseContent;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new StringContent(_responseContent, Encoding.UTF8, "application/xml") };
            return Task.FromResult(response);
        }
    }
}
