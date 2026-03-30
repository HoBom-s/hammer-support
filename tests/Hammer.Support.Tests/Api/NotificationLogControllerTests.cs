using FluentAssertions;
using Hammer.Support.Api.Controllers;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Hammer.Support.Tests.Api;

public sealed class NotificationLogControllerTests
{
    private readonly INotificationLogRepository _repo = Substitute.For<INotificationLogRepository>();
    private readonly NotificationLogController _sut;

    public NotificationLogControllerTests()
    {
        _sut = new NotificationLogController(_repo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByRecipientAsync_EmptyToken_ReturnsBadRequest(string? token)
    {
        IActionResult result = await _sut.GetByRecipientAsync(token!, cancellationToken: CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetByRecipientAsync_ValidToken_ReturnsOkWithLogs()
    {
        var logs = new List<NotificationLog>
        {
            NotificationLog.CreateLog(Guid.NewGuid(), "token-abc", "Title", "Body", NotificationChannel.InApp),
        };
        _repo.GetByRecipientAsync("token-abc", 20, Arg.Any<CancellationToken>()).Returns(logs);

        IActionResult result = await _sut.GetByRecipientAsync("token-abc", cancellationToken: CancellationToken.None);

        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(logs);
    }

    [Fact]
    public async Task GetByRecipientAsync_LimitExceeds100_ClampedTo100()
    {
        _repo.GetByRecipientAsync("token", 100, Arg.Any<CancellationToken>())
            .Returns(new List<NotificationLog>());

        await _sut.GetByRecipientAsync("token", limit: 999, cancellationToken: CancellationToken.None);

        await _repo.Received(1).GetByRecipientAsync("token", 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByRecipientAsync_LimitBelowOne_ClampedToOne()
    {
        _repo.GetByRecipientAsync("token", 1, Arg.Any<CancellationToken>())
            .Returns(new List<NotificationLog>());

        await _sut.GetByRecipientAsync("token", limit: -5, cancellationToken: CancellationToken.None);

        await _repo.Received(1).GetByRecipientAsync("token", 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByRecipientAsync_NoLogs_ReturnsEmptyOk()
    {
        _repo.GetByRecipientAsync("unknown", 20, Arg.Any<CancellationToken>())
            .Returns(new List<NotificationLog>());

        IActionResult result = await _sut.GetByRecipientAsync("unknown", cancellationToken: CancellationToken.None);

        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        IReadOnlyList<NotificationLog> logs = okResult.Value.Should().BeAssignableTo<IReadOnlyList<NotificationLog>>().Subject;
        logs.Should().BeEmpty();
    }
}
