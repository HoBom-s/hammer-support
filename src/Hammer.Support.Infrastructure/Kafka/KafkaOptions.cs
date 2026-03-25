namespace Hammer.Support.Infrastructure.Kafka;

/// <summary>
/// Configuration options for Kafka producer.
/// </summary>
public sealed class KafkaOptions
{
    /// <summary>
    /// Configuration section name in appsettings.
    /// </summary>
    public const string SectionName = "Kafka";

    /// <summary>
    /// Gets or sets comma-separated list of Kafka broker addresses.
    /// </summary>
    public string BootstrapServers { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets delay in milliseconds to wait for batching messages.
    /// </summary>
    public int LingerMs { get; set; } = 5;

    /// <summary>
    /// Gets or sets maximum number of messages to batch before sending.
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
