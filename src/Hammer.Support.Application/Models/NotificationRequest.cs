namespace Hammer.Support.Application.Models;

/// <summary>
/// Incoming notification request consumed from Kafka.
/// </summary>
public sealed record NotificationRequest
{
    /// <summary>Gets the template key to look up (e.g. "auction_new_item").</summary>
    public required string TemplateKey { get; init; }

    /// <summary>Gets the FCM device token or user identifier.</summary>
    public required string RecipientToken { get; init; }

    /// <summary>Gets the template variables for placeholder substitution.</summary>
    public required Dictionary<string, string> Variables { get; init; }
}
