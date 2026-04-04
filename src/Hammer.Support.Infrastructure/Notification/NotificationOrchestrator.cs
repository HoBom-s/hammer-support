using System.Diagnostics.CodeAnalysis;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Hammer.Support.Infrastructure.Notification;

/// <summary>
///     Orchestrates notification processing: template lookup, rendering, delivery, and logging.
/// </summary>
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Notification processing, logging overhead negligible")]
internal sealed class NotificationOrchestrator : INotificationOrchestrator
{
    private readonly ILogger<NotificationOrchestrator> _logger;
    private readonly INotificationLogRepository _logRepo;
    private readonly Dictionary<NotificationChannel, INotificationSender> _senders;
    private readonly INotificationTemplateRepository _templateRepo;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationOrchestrator" /> class.
    /// </summary>
    /// <param name="templateRepo">Notification template repository.</param>
    /// <param name="logRepo">Notification log repository.</param>
    /// <param name="senders">Registered notification senders.</param>
    /// <param name="logger">Logger instance.</param>
    public NotificationOrchestrator(
        INotificationTemplateRepository templateRepo,
        INotificationLogRepository logRepo,
        IEnumerable<INotificationSender> senders,
        ILogger<NotificationOrchestrator> logger)
    {
        _templateRepo = templateRepo;
        _logRepo = logRepo;
        _senders = senders.ToDictionary(s => s.Channel);
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ProcessAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        NotificationTemplate? notificationTemplate = await _templateRepo.GetByKeyAsync(request.TemplateKey, cancellationToken);

        if (notificationTemplate is null)
        {
            _logger.LogWarning("Template not found: {TemplateKey}", request.TemplateKey);
            return;
        }

        var title = TemplateRenderer.Render(notificationTemplate.TitleTemplate, request.Variables, out IReadOnlyList<string> titleUnmatched);
        var body = TemplateRenderer.Render(notificationTemplate.BodyTemplate, request.Variables, out IReadOnlyList<string> bodyUnmatched);

        if (titleUnmatched.Count > 0 || bodyUnmatched.Count > 0)
        {
            var all = titleUnmatched.Concat(bodyUnmatched).Distinct().ToList();
            _logger.LogWarning("Unmatched placeholders for template {TemplateKey}: {Placeholders}", request.TemplateKey, string.Join(", ", all));
        }

        IReadOnlyList<NotificationChannel> channels = notificationTemplate.Channel.Resolve();

        foreach (NotificationChannel channel in channels)
        {
            if (!_senders.TryGetValue(channel, out INotificationSender? sender))
            {
                _logger.LogWarning("No sender registered for channel {Channel}", channel);
                continue;
            }

            await SendAndLogAsync(sender, notificationTemplate.Id, request.RecipientToken, title, body, cancellationToken);
        }
    }

    private async Task SendAndLogAsync(
        INotificationSender sender,
        Guid templateId,
        string recipientToken,
        string title,
        string body,
        CancellationToken ct)
    {
        var log = NotificationLog.CreateLog(templateId, recipientToken, title, body, sender.Channel);

        // Persist Pending log first — ensures audit trail even if send or update fails.
        await _logRepo.SaveAsync(log, ct);

        try
        {
            await sender.SendAsync(recipientToken, title, body, ct);
            log.ChangeNotificationStatusToSent();
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _logger.LogError(ex, "Delivery failed on {Channel} for {Token}", sender.Channel, recipientToken[..Math.Min(recipientToken.Length, 12)] + "...");
            log.ChangeNotificationStatusToFailed(ex.Message);
        }

        try
        {
            await _logRepo.UpdateAsync(log, ct);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _logger.LogError(ex, "Failed to update notification log {LogId} to {Status}", log.Id, log.Status);
        }
    }
}
