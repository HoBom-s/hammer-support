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
    private readonly INotificationSender _pushSender = Substitute.For<INotificationSender>();
    private readonly INotificationSender _inAppSender = Substitute.For<INotificationSender>();
    private readonly NotificationOrchestrator _sut;

    public NotificationOrchestratorTests()
    {
        _pushSender.Channel.Returns(NotificationChannel.Push);
        _inAppSender.Channel.Returns(NotificationChannel.InApp);

        _sut = new NotificationOrchestrator(
            _templateRepo,
            _logRepo,
            [_pushSender, _inAppSender],
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

        await _pushSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _logRepo.DidNotReceive().SaveAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_PushChannel_SavesPendingThenUpdatesSent()
    {
        NotificationTemplate template = CreateTemplate(NotificationChannel.Push);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);

        await _sut.ProcessAsync(CreateRequest());

        await _pushSender.Received(1).SendAsync("device-token-abc", "Hello Fox", "Welcome Fox!", Arg.Any<CancellationToken>());

        // SaveAsync is called first with Pending status (before send)
        await _logRepo.Received(1).SaveAsync(
            Arg.Is<NotificationLog>(l => l.Channel == NotificationChannel.Push),
            Arg.Any<CancellationToken>());

        // UpdateAsync is called after send with final Sent status
        await _logRepo.Received(1).UpdateAsync(
            Arg.Is<NotificationLog>(l => l.Status == NotificationStatus.Sent && l.Channel == NotificationChannel.Push),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_BothChannel_SendsToPushAndInApp()
    {
        NotificationTemplate template = CreateTemplate(NotificationChannel.Both);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);

        await _sut.ProcessAsync(CreateRequest());

        await _pushSender.Received(1).SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _inAppSender.Received(1).SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _logRepo.Received(2).SaveAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>());
        await _logRepo.Received(2).UpdateAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_SenderThrows_LogsFailedStatus()
    {
        NotificationTemplate template = CreateTemplate(NotificationChannel.Push);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);
        _pushSender.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Push unavailable"));

        await _sut.ProcessAsync(CreateRequest());

        // SaveAsync called first with Pending
        await _logRepo.Received(1).SaveAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>());

        // UpdateAsync called with Failed status
        await _logRepo.Received(1).UpdateAsync(
            Arg.Is<NotificationLog>(l => l.Status == NotificationStatus.Failed && l.ErrorMessage == "Push unavailable"),
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
        // Create orchestrator with only InApp sender (no Push sender)
        NotificationOrchestrator orchestrator = new(
            _templateRepo,
            _logRepo,
            [_inAppSender],
            Substitute.For<ILogger<NotificationOrchestrator>>());

        NotificationTemplate template = CreateTemplate(NotificationChannel.Push);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);

        await orchestrator.ProcessAsync(CreateRequest());

        await _inAppSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _logRepo.DidNotReceive().SaveAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_SaveAsyncThrows_PropagatesException()
    {
        NotificationTemplate template = CreateTemplate(NotificationChannel.Push);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);
        _logRepo.SaveAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("DB connection lost"));

        Func<Task> act = () => _sut.ProcessAsync(CreateRequest());

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB connection lost");
        await _pushSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_UpdateAsyncThrows_DoesNotCrash()
    {
        NotificationTemplate template = CreateTemplate(NotificationChannel.Push);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);
        _logRepo.UpdateAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("DB timeout"));

        Func<Task> act = () => _sut.ProcessAsync(CreateRequest());

        await act.Should().NotThrowAsync();
        await _pushSender.Received(1).SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_SaveCalledBeforeSend()
    {
        NotificationTemplate template = CreateTemplate(NotificationChannel.Push);
        _templateRepo.GetByKeyAsync("test_key", Arg.Any<CancellationToken>()).Returns(template);

        var callOrder = new List<string>();
        _logRepo.SaveAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(_ => callOrder.Add("Save"));
        _pushSender.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(_ => callOrder.Add("Send"));
        _logRepo.UpdateAsync(Arg.Any<NotificationLog>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(_ => callOrder.Add("Update"));

        await _sut.ProcessAsync(CreateRequest());

        callOrder.Should().ContainInOrder("Save", "Send", "Update");
    }

    private static NotificationRequest CreateRequest(string templateKey = "test_key", string token = "device-token-abc") =>
        new()
        {
            TemplateKey = templateKey,
            RecipientToken = token,
            Variables = new Dictionary<string, string> { ["name"] = "Fox" },
        };

    private static NotificationTemplate CreateTemplate(NotificationChannel channel = NotificationChannel.Push) =>
        new()
        {
            Id = Guid.NewGuid(),
            TemplateKey = "test_key",
            TitleTemplate = "Hello {name}",
            BodyTemplate = "Welcome {name}!",
            Channel = channel,
        };
}
