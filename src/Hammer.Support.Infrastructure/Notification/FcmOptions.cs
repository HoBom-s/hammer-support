namespace Hammer.Support.Infrastructure.Notification;

/// <summary>
/// Configuration options for Firebase Cloud Messaging.
/// </summary>
public sealed class FcmOptions
{
    /// <summary>
    /// Configuration section name in appsettings.
    /// </summary>
    public const string SectionName = "Fcm";

    /// <summary>
    /// Gets or sets the file path to the Firebase service account JSON credential.
    /// </summary>
    public string CredentialPath { get; set; } = string.Empty;
}
