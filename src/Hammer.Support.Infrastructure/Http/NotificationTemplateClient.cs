using System.Net;
using System.Text.Json;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Microsoft.Extensions.Logging;

namespace Hammer.Support.Infrastructure.Http;

/// <summary>
/// HTTP client for retrieving notification templates from hammer-internal.
/// </summary>
internal sealed class NotificationTemplateClient(
    HttpClient httpClient,
    ILogger<NotificationTemplateClient> logger) : INotificationTemplateClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <inheritdoc />
    public async Task<NotificationTemplateDto?> GetByKeyAsync(
        string templateKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(
                new Uri($"internal/notification-templates/by-key/{Uri.EscapeDataString(templateKey)}", UriKind.Relative),
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<NotificationTemplateDto>(json, JsonOptions);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch notification template by key {TemplateKey}", templateKey);
            return null;
        }
    }
}
