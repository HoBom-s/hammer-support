using System.Diagnostics.CodeAnalysis;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Notification;

/// <summary>
/// Sends push notifications via Firebase Admin SDK.
/// </summary>
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Notification sending, logging overhead negligible")]
internal sealed class FcmSender : INotificationSender
{
    private readonly ILogger<FcmSender> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FcmSender"/> class.
    /// </summary>
    /// <param name="options">FCM configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public FcmSender(IOptions<FcmOptions> options, ILogger<FcmSender> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;

        if (FirebaseApp.DefaultInstance is null)
        {
            using FileStream stream = File.OpenRead(options.Value.CredentialPath);
            var credential = ServiceAccountCredential.FromServiceAccountData(stream);

            FirebaseApp.Create(new AppOptions
            {
                Credential = credential.ToGoogleCredential(),
            });
        }
    }

    /// <inheritdoc />
    public NotificationChannel Channel => NotificationChannel.Fcm;

    /// <inheritdoc />
    public async Task SendAsync(string recipientToken, string title, string body, CancellationToken cancellationToken = default)
    {
        Message message = new()
        {
            Token = recipientToken,
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = title,
                Body = body,
            },
        };

        var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
        _logger.LogInformation("FCM sent: {MessageId} to {Token}", messageId, recipientToken[..Math.Min(recipientToken.Length, 12)] + "...");
    }
}
