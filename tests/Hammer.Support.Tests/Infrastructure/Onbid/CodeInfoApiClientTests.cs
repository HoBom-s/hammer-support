using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Onbid;
using Hammer.Support.Infrastructure.Onbid.CodeInfo;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Tests.Infrastructure.Onbid;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Test lifecycle managed by xUnit")]
public sealed class CodeInfoApiClientTests
{
    private static readonly OnbidOptions _defaultOptions = new() { ServiceKey = "test-key", CodeBaseUrl = "http://localhost", PageSize = 100 };

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

        CodeInfoApiClient client = CreateClient(xml);

        CodeInfoPageResult result = await client.FetchPageAsync(1, 100);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);

        OnbidCodeItem first = result.Items[0];
        Assert.Equal("001", first.CtgrId);
        Assert.Equal("토지", first.CtgrNm);
        Assert.Equal("000", first.CtgrHirkId);
        Assert.Equal("전체", first.CtgrHirkNm);

        OnbidCodeItem second = result.Items[1];
        Assert.Equal("002", second.CtgrId);
        Assert.Equal("건물", second.CtgrNm);
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

        CodeInfoApiClient client = CreateClient(xml);

        CodeInfoPageResult result = await client.FetchPageAsync(1, 100);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    private static CodeInfoApiClient CreateClient(string xmlResponse)
    {
        FakeHttpMessageHandler handler = new(xmlResponse);
        HttpClient httpClient = new(handler);
        IOptions<OnbidOptions> options = Options.Create(_defaultOptions);

        return new CodeInfoApiClient(httpClient, options, NullLogger<CodeInfoApiClient>.Instance);
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
