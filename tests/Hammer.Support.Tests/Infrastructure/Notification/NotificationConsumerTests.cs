using FluentAssertions;
using Hammer.Support.Application.Models;
using Hammer.Support.Infrastructure.Notification;

namespace Hammer.Support.Tests.Infrastructure.Notification;

public sealed class NotificationConsumerTests
{
    [Fact]
    public void IsValidRequest_Null_ReturnsFalse()
    {
        NotificationConsumer.IsValidRequest(null).Should().BeFalse();
    }

    [Fact]
    public void IsValidRequest_EmptyTemplateKey_ReturnsFalse()
    {
        var request = new NotificationRequest
        {
            TemplateKey = string.Empty,
            RecipientToken = "token",
            Variables = new Dictionary<string, string>(),
        };

        NotificationConsumer.IsValidRequest(request).Should().BeFalse();
    }

    [Fact]
    public void IsValidRequest_EmptyRecipientToken_ReturnsFalse()
    {
        var request = new NotificationRequest
        {
            TemplateKey = "key",
            RecipientToken = "  ",
            Variables = new Dictionary<string, string>(),
        };

        NotificationConsumer.IsValidRequest(request).Should().BeFalse();
    }

    [Fact]
    public void IsValidRequest_WhitespaceTemplateKey_ReturnsFalse()
    {
        var request = new NotificationRequest
        {
            TemplateKey = "   ",
            RecipientToken = "token",
            Variables = new Dictionary<string, string>(),
        };

        NotificationConsumer.IsValidRequest(request).Should().BeFalse();
    }

    [Fact]
    public void IsValidRequest_ValidRequest_ReturnsTrue()
    {
        var request = new NotificationRequest
        {
            TemplateKey = "auction_new_item",
            RecipientToken = "device-token-abc",
            Variables = new Dictionary<string, string> { ["name"] = "Fox" },
        };

        NotificationConsumer.IsValidRequest(request).Should().BeTrue();
    }
}
