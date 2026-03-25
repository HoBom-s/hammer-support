using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Onbid;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Tests.Infrastructure.Onbid;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Test lifecycle managed by xUnit")]
public sealed class KamcoApiClientTests
{
    private static readonly OnbidOptions _defaultOptions = new() { ServiceKey = "test-key", KamcoBaseUrl = "http://localhost", PageSize = 100 };

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
                                           <PBCT_CDTN_NO>400</PBCT_CDTN_NO>
                                           <CLTR_NO>300</CLTR_NO>
                                           <CLTR_HSTR_NO>500</CLTR_HSTR_NO>
                                           <CLTR_MNMT_NO>2025-00001-001</CLTR_MNMT_NO>
                                           <CLTR_NM>서울 강남구</CLTR_NM>
                                           <CTGR_FULL_NM>토지 / 대지</CTGR_FULL_NM>
                                           <LDNM_ADRS>서울 강남구 123</LDNM_ADRS>
                                           <NMRD_ADRS>테헤란로 1</NMRD_ADRS>
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
                                   </items>
                               </body>
                           </response>
                           """;

        KamcoApiClient client = CreateClient(xml);

        KamcoPageResult result = await client.FetchPageAsync(1, 100);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);

        KamcoAuctionItem item = result.Items[0];
        Assert.Equal(100, item.PlnmNo);
        Assert.Equal(200, item.PbctNo);
        Assert.Equal(300, item.CltrNo);
        Assert.Equal("서울 강남구", item.CltrNm);
        Assert.Equal("토지 / 대지", item.CtgrFullNm);
        Assert.Equal(500000000, item.MinBidPrc);
        Assert.Equal(700000000, item.ApslAsesAvgAmt);
        Assert.Equal("20260325100000", item.PbctBegnDtm);
        Assert.Equal("20260327170000", item.PbctClsDtm);
        Assert.Equal(3, item.UscbdCnt);
        Assert.Equal(42, item.IqryCnt);
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

        KamcoApiClient client = CreateClient(xml);

        KamcoPageResult result = await client.FetchPageAsync(1, 100);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    private static KamcoApiClient CreateClient(string xmlResponse)
    {
        FakeHttpMessageHandler handler = new(xmlResponse);
        HttpClient httpClient = new(handler);
        IOptions<OnbidOptions> options = Options.Create(_defaultOptions);

        return new KamcoApiClient(httpClient, options, NullLogger<KamcoApiClient>.Instance);
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
