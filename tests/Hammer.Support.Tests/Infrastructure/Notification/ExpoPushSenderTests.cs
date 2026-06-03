using System.Net;
using FluentAssertions;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Notification;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Hammer.Support.Tests.Infrastructure.Notification;

public sealed class ExpoPushSenderTests : IDisposable
{
    private readonly StubHttpMessageHandler _handler = new(HttpStatusCode.OK);
    private readonly HttpClient _httpClient;
    private readonly ExpoPushSender _sut;

    public ExpoPushSenderTests()
    {
        _httpClient = new HttpClient(_handler);
        _sut = new ExpoPushSender(_httpClient, Substitute.For<ILogger<ExpoPushSender>>());
    }

    [Fact]
    public void Channel_ReturnsPush()
    {
        _sut.Channel.Should().Be(NotificationChannel.Push);
    }

    [Fact]
    public async Task SendAsync_SuccessResponse_CompletesWithoutException()
    {
        Func<Task> act = () => _sut.SendAsync("ExponentPushToken[abc123]", "Title", "Body");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendAsync_ErrorResponse_ThrowsHttpRequestException()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.InternalServerError);
        using var httpClient = new HttpClient(handler);
        var sut = new ExpoPushSender(httpClient, Substitute.For<ILogger<ExpoPushSender>>());

        Func<Task> act = () => sut.SendAsync("ExponentPushToken[abc123]", "Title", "Body");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _handler.Dispose();
    }

    internal sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public StubHttpMessageHandler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(_statusCode));
    }
}
