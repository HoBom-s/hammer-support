using FluentAssertions;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Hammer.Support.Infrastructure.Notification;
using NSubstitute;

namespace Hammer.Support.Tests.Infrastructure.Notification;

public sealed class NotificationTemplateServiceTests
{
    private readonly INotificationTemplateRepository _repo = Substitute.For<INotificationTemplateRepository>();
    private readonly NotificationTemplateService _sut;

    public NotificationTemplateServiceTests()
    {
        _sut = new NotificationTemplateService(_repo);
    }

    [Fact]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        IReadOnlyList<NotificationTemplate> expected = [new() { TemplateKey = "key1" }];
        _repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expected);

        IReadOnlyList<NotificationTemplate> result = await _sut.GetAllAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetByIdAsync_DelegatesToRepository()
    {
        var id = Guid.NewGuid();
        var expected = new NotificationTemplate { Id = id, TemplateKey = "key1" };
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(expected);

        NotificationTemplate? result = await _sut.GetByIdAsync(id);

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task CreateAsync_NullCommand_Throws()
    {
        Func<Task> act = () => _sut.CreateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_MapsCommandToEntity()
    {
        var command = new CreateNotificationTemplateCommand
        {
            TemplateKey = "auction_new",
            TitleTemplate = "New auction: {name}",
            BodyTemplate = "Check out {name}",
            Channel = NotificationChannel.Both,
        };
        _repo.CreateAsync(Arg.Any<NotificationTemplate>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<NotificationTemplate>());

        NotificationTemplate result = await _sut.CreateAsync(command);

        result.TemplateKey.Should().Be("auction_new");
        result.TitleTemplate.Should().Be("New auction: {name}");
        result.BodyTemplate.Should().Be("Check out {name}");
        result.Channel.Should().Be(NotificationChannel.Both);
    }

    [Fact]
    public async Task UpdateAsync_NullCommand_Throws()
    {
        Func<Task> act = () => _sut.UpdateAsync(Guid.NewGuid(), null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((NotificationTemplate?)null);

        var command = new UpdateNotificationTemplateCommand
        {
            TemplateKey = "k",
            TitleTemplate = "t",
            BodyTemplate = "b",
            Channel = NotificationChannel.Fcm,
        };
        NotificationTemplate? result = await _sut.UpdateAsync(id, command);

        result.Should().BeNull();
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<NotificationTemplate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_Found_UpdatesFieldsAndSaves()
    {
        var id = Guid.NewGuid();
        var existing = new NotificationTemplate
        {
            Id = id,
            TemplateKey = "old",
            TitleTemplate = "Old",
            BodyTemplate = "Old",
            Channel = NotificationChannel.Fcm,
        };
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);
        _repo.UpdateAsync(Arg.Any<NotificationTemplate>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<NotificationTemplate>());

        var command = new UpdateNotificationTemplateCommand
        {
            TemplateKey = "new_key",
            TitleTemplate = "New Title",
            BodyTemplate = "New Body",
            Channel = NotificationChannel.InApp,
        };
        NotificationTemplate? result = await _sut.UpdateAsync(id, command);

        result.Should().NotBeNull();
        result!.TemplateKey.Should().Be("new_key");
        result.TitleTemplate.Should().Be("New Title");
        result.BodyTemplate.Should().Be("New Body");
        result.Channel.Should().Be(NotificationChannel.InApp);
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var id = Guid.NewGuid();
        _repo.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.DeleteAsync(id);

        result.Should().BeTrue();
    }
}
