using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using FluentAssertions;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Naver;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Tests.Infrastructure.Naver;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Test lifecycle managed by xUnit")]
public sealed class NaverNewsApiClientTests
{
    private static readonly NaverOptions _defaultOptions = new() { ClientId = "test-id", ClientSecret = "test-secret" };

    [Fact]
    public async Task SearchAsync_ValidResponse_ReturnsMappedArticles()
    {
        const string json = """
                            {
                                "lastBuildDate": "Mon, 02 Jun 2025 09:00:00 +0900",
                                "total": 1,
                                "start": 1,
                                "display": 1,
                                "items": [
                                    {
                                        "title": "<b>경매</b> 뉴스",
                                        "originallink": "https://news.example.com/1",
                                        "link": "https://n.news.naver.com/1",
                                        "description": "설명",
                                        "pubDate": "Mon, 02 Jun 2025 09:00:00 +0900"
                                    }
                                ]
                            }
                            """;

        NaverNewsApiClient client = CreateClient(json);

        IReadOnlyList<NewsArticle> result = await client.SearchAsync("경매");

        result.Should().ContainSingle();
        NewsArticle article = result[0];
        article.Title.Should().Be("<b>경매</b> 뉴스");
        article.OriginalLink.Should().Be("https://news.example.com/1");
        article.Link.Should().Be("https://n.news.naver.com/1");
        article.Description.Should().Be("설명");
    }

    [Fact]
    public async Task SearchAsync_ParsesRfc1123PubDate()
    {
        const string json = """
                            {
                                "total": 1,
                                "items": [
                                    {
                                        "title": "t",
                                        "originallink": "https://news.example.com/1",
                                        "link": "https://n.news.naver.com/1",
                                        "description": "d",
                                        "pubDate": "Mon, 02 Jun 2025 09:00:00 +0900"
                                    }
                                ]
                            }
                            """;

        NaverNewsApiClient client = CreateClient(json);

        IReadOnlyList<NewsArticle> result = await client.SearchAsync("경매");

        result[0].PubDate.Should().Be(new DateTimeOffset(2025, 6, 2, 9, 0, 0, TimeSpan.FromHours(9)));
    }

    [Fact]
    public async Task SearchAsync_EmptyItems_ReturnsEmptyList()
    {
        const string json = """
                            {
                                "total": 0,
                                "items": []
                            }
                            """;

        NaverNewsApiClient client = CreateClient(json);

        IReadOnlyList<NewsArticle> result = await client.SearchAsync("경매");

        result.Should().BeEmpty();
    }

    private static NaverNewsApiClient CreateClient(string jsonResponse)
    {
        FakeHttpMessageHandler handler = new(jsonResponse);
        HttpClient httpClient = new(handler);
        IOptions<NaverOptions> options = Options.Create(_defaultOptions);

        return new NaverNewsApiClient(httpClient, options, NullLogger<NaverNewsApiClient>.Instance);
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
            HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new StringContent(_responseContent, Encoding.UTF8, "application/json") };
            return Task.FromResult(response);
        }
    }
}
