namespace Hammer.Support.Application;

/// <summary>
/// Kafka topic names shared between producers and consumers.
/// </summary>
public static class KafkaTopics
{
    /// <summary>
    /// KAMCO public auction items.
    /// </summary>
    public const string KamcoAuction = "onbid-kamco-auction";

    /// <summary>
    /// Institution public sale announcements.
    /// </summary>
    public const string InstitutionAuction = "onbid-institution-auction";

    /// <summary>
    /// Onbid category code information.
    /// </summary>
    public const string CodeInfo = "onbid-code-info";

    /// <summary>
    /// Real estate market trade prices from MOLIT.
    /// </summary>
    public const string RealEstatePrice = "real-estate-market-price";

    /// <summary>
    /// Incoming notification requests from other services.
    /// </summary>
    public const string NotificationRequest = "notification-request";
}
