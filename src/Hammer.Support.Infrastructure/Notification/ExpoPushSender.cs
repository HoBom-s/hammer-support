using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Hammer.Support.Infrastructure.Notification;

/// <summary>
/// Sends push notifications via the Expo Push API.
/// </summary>
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Notification sending, logging overhead negligible")]
internal sealed class ExpoPushSender : INotificationSender
{
    private static readonly Uri _expoPushEndpoint = new("https://exp.host/--/api/v2/push/send");

    private readonly HttpClient _httpClient;
    private readonly ILogger<ExpoPushSender> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpoPushSender"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for Expo Push API.</param>
    /// <param name="logger">Logger instance.</param>
    public ExpoPushSender(HttpClient httpClient, ILogger<ExpoPushSender> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public NotificationChannel Channel => NotificationChannel.Push;

    /// <inheritdoc />
    public async Task SendAsync(string recipientToken, string title, string body, CancellationToken cancellationToken = default)
    {
        var payload = new ExpoPushMessage
        {
            To = recipientToken,
            Title = title,
            Body = body,
            Sound = "default",
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            _expoPushEndpoint,
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation(
            "Expo push sent to {Token}",
            recipientToken[..Math.Min(recipientToken.Length, 12)] + "...");
    }

    private sealed class ExpoPushMessage
    {
        [JsonPropertyName("to")]
        public string To { get; init; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; init; } = string.Empty;

        [JsonPropertyName("sound")]
        public string Sound { get; init; } = string.Empty;
    }
}
