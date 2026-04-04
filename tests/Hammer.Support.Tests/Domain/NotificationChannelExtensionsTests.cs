using FluentAssertions;
using Hammer.Support.Domain.Models;

namespace Hammer.Support.Tests.Domain;

public sealed class NotificationChannelExtensionsTests
{
    [Fact]
    public void Resolve_Push_ReturnsSinglePush()
    {
        IReadOnlyList<NotificationChannel> result = NotificationChannel.Push.Resolve();

        result.Should().ContainSingle().Which.Should().Be(NotificationChannel.Push);
    }

    [Fact]
    public void Resolve_InApp_ReturnsSingleInApp()
    {
        IReadOnlyList<NotificationChannel> result = NotificationChannel.InApp.Resolve();

        result.Should().ContainSingle().Which.Should().Be(NotificationChannel.InApp);
    }

    [Fact]
    public void Resolve_Both_ReturnsPushAndInApp()
    {
        IReadOnlyList<NotificationChannel> result = NotificationChannel.Both.Resolve();

        result.Should().HaveCount(2);
        result.Should().ContainInOrder(NotificationChannel.Push, NotificationChannel.InApp);
    }
}
