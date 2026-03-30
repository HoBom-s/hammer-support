using FluentAssertions;
using Hammer.Support.Domain.Models;

namespace Hammer.Support.Tests.Domain;

public sealed class NotificationChannelExtensionsTests
{
    [Fact]
    public void Resolve_Fcm_ReturnsSingleFcm()
    {
        IReadOnlyList<NotificationChannel> result = NotificationChannel.Fcm.Resolve();

        result.Should().ContainSingle().Which.Should().Be(NotificationChannel.Fcm);
    }

    [Fact]
    public void Resolve_InApp_ReturnsSingleInApp()
    {
        IReadOnlyList<NotificationChannel> result = NotificationChannel.InApp.Resolve();

        result.Should().ContainSingle().Which.Should().Be(NotificationChannel.InApp);
    }

    [Fact]
    public void Resolve_Both_ReturnsFcmAndInApp()
    {
        IReadOnlyList<NotificationChannel> result = NotificationChannel.Both.Resolve();

        result.Should().HaveCount(2);
        result.Should().ContainInOrder(NotificationChannel.Fcm, NotificationChannel.InApp);
    }
}
