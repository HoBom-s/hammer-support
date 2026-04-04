using FluentAssertions;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Notification;

namespace Hammer.Support.Tests.Infrastructure.Notification;

public sealed class InAppNotificationSenderTests
{
    private readonly InAppNotificationSender _sut = new();

    [Fact]
    public void Channel_ReturnsInApp()
    {
        _sut.Channel.Should().Be(NotificationChannel.InApp);
    }

    [Fact]
    public async Task SendAsync_CompletesImmediately()
    {
        Task task = _sut.SendAsync("token", "title", "body");

        task.IsCompleted.Should().BeTrue();
        await task;
    }
}
