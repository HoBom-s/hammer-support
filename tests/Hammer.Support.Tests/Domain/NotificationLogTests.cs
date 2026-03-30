using FluentAssertions;
using Hammer.Support.Domain.Models;

namespace Hammer.Support.Tests.Domain;

public sealed class NotificationLogTests
{
    [Fact]
    public void CreateLog_SetsFieldsAndPendingStatus()
    {
        var templateId = Guid.NewGuid();

        var log = NotificationLog.CreateLog(templateId, "token-123", "Title", "Body", NotificationChannel.Fcm);

        log.TemplateId.Should().Be(templateId);
        log.RecipientToken.Should().Be("token-123");
        log.Title.Should().Be("Title");
        log.Body.Should().Be("Body");
        log.Channel.Should().Be(NotificationChannel.Fcm);
        log.Status.Should().Be(NotificationStatus.Pending);
        log.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ChangeNotificationStatusToSent_SetsStatusToSent()
    {
        var log = NotificationLog.CreateLog(Guid.NewGuid(), "t", "T", "B", NotificationChannel.InApp);

        log.ChangeNotificationStatusToSent();

        log.Status.Should().Be(NotificationStatus.Sent);
    }

    [Fact]
    public void ChangeNotificationStatusToFailed_SetsStatusAndErrorMessage()
    {
        var log = NotificationLog.CreateLog(Guid.NewGuid(), "t", "T", "B", NotificationChannel.Fcm);

        log.ChangeNotificationStatusToFailed("connection timeout");

        log.Status.Should().Be(NotificationStatus.Failed);
        log.ErrorMessage.Should().Be("connection timeout");
    }
}
