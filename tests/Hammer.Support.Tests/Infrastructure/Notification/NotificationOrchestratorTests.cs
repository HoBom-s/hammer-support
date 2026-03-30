using FluentAssertions;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Notification;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hammer.Support.Tests.Infrastructure.Notification;

public sealed class NotificationOrchestratorTests
{
    private readonly INotificationTemplateRepository _templateRepo = Substitute.For<INotificationTemplateRepository>();
    private readonly INotificationLogRepository _logRepo = Substitute.For<INotificationLogRepository>();
    private readonly INotificationSender _fcmSender = Substitute.For<INotificationSender>();
    private readonly INotificationSender _inAppSender = Substitute.For<INotificationSender>();
    private readonly NotificationOrchestrator _sut;

    public NotificationOrchestratorTests()
    {
        _fcmSender.Channel.Returns(NotificationChannel.Fcm);
        _inAppSender.Channel.Returns(NotificationChannel.InApp);

        _sut = new NotificationOrchestrator(
            _templateRepo,
            _logRepo,
            [_fcmSender, _inAppSender],
            Substitute.For<ILogger<NotificationOrchestrator>>());
    }

    [Fact]
    public async Task ProcessAsync_NullRequest_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _sut.ProcessAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessAsync_TemplateNotFound_ReturnsWithoutSending()
    {
        _templateRepo.GetByKeyAsync("missing", Arg.Any<CancellationToken>())
            .Returns((NotificationTemplate?)null);

        await _sut.ProcessAsync(CreateRequest("missing"));

        await _fcmSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _logRepo.DidNotReceive().SaveAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_FcmChannel_SendsAndLogsSuccess()
    {
        NotificationTemplate template = CreateTemplate(NotificationChannel.Fcm);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);

        await _sut.ProcessAsync(CreateRequest());

        await _fcmSender.Received(1).SendAsync("device-token-abc", "Hello Fox", "Welcome Fox!", Arg.Any<CancellationToken>());
        await _logRepo.Received(1).SaveAsync(
            Arg.Is<NotificationLog>(l => l.Status == NotificationStatus.Sent && l.Channel == NotificationChannel.Fcm),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_BothChannel_SendsToFcmAndInApp()
    {
        NotificationTemplate template = CreateTemplate(NotificationChannel.Both);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);

        await _sut.ProcessAsync(CreateRequest());

        await _fcmSender.Received(1).SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _inAppSender.Received(1).SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _logRepo.Received(2).SaveAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_SenderThrows_LogsFailedStatus()
    {
        NotificationTemplate template = CreateTemplate(NotificationChannel.Fcm);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);
        _fcmSender.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("FCM unavailable"));

        await _sut.ProcessAsync(CreateRequest());

        await _logRepo.Received(1).SaveAsync(
            Arg.Is<NotificationLog>(l => l.Status == NotificationStatus.Failed && l.ErrorMessage == "FCM unavailable"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_RendersTemplateVariables()
    {
        NotificationTemplate template = CreateTemplate(NotificationChannel.InApp);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);

        await _sut.ProcessAsync(CreateRequest());

        await _inAppSender.Received(1).SendAsync("device-token-abc", "Hello Fox", "Welcome Fox!", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_NoSenderForChannel_SkipsWithoutError()
    {
        // Create orchestrator with only InApp sender (no Fcm sender)
        NotificationOrchestrator orchestrator = new(
            _templateRepo,
            _logRepo,
            [_inAppSender],
            Substitute.For<ILogger<NotificationOrchestrator>>());

        NotificationTemplate template = CreateTemplate(NotificationChannel.Fcm);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);

        await orchestrator.ProcessAsync(CreateRequest());

        await _inAppSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _logRepo.DidNotReceive().SaveAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>());
    }

    private static NotificationRequest CreateRequest(string templateKey = "test_key", string token = "device-token-abc") =>
        new()
        {
            TemplateKey = templateKey,
            RecipientToken = token,
            Variables = new Dictionary<string, string> { ["name"] = "Fox" },
        };

    private static NotificationTemplate CreateTemplate(NotificationChannel channel = NotificationChannel.Fcm) =>
        new()
        {
            Id = Guid.NewGuid(),
            TemplateKey = "test_key",
            TitleTemplate = "Hello {name}",
            BodyTemplate = "Welcome {name}!",
            Channel = channel,
        };
}
