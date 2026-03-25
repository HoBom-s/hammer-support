using System.Diagnostics.CodeAnalysis;
using Confluent.Kafka;
using Hammer.Support.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Kafka;

/// <summary>
/// Kafka-based implementation of <see cref="IEventPublisher"/>.
/// Uses fire-and-forget <see cref="IProducer{TKey,TValue}.Produce"/> for non-blocking enqueue
/// and tracks delivery failures via delivery reports.
/// </summary>
public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;
    private int _failedCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaEventPublisher"/> class.
    /// </summary>
    /// <param name="options">Kafka configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public KafkaEventPublisher(IOptions<KafkaOptions> options, ILogger<KafkaEventPublisher> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;

        KafkaOptions kafkaOptions = options.Value;

        ProducerConfig config = new()
        {
            BootstrapServers = kafkaOptions.BootstrapServers,
            Acks = Acks.Leader,
            LingerMs = kafkaOptions.LingerMs,
            BatchNumMessages = kafkaOptions.BatchSize,
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    /// <inheritdoc />
    [SuppressMessage("Reliability", "CA1849:Call async methods when in an async method", Justification = "Produce() is non-blocking enqueue; ProduceAsync() awaits broker ACK which defeats batching")]
    public Task PublishAsync(string topic, string key, string value, CancellationToken cancellationToken = default)
    {
        Message<string, string> message = new() { Key = key, Value = value };
        _producer.Produce(topic, message, HandleDeliveryReport);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> FlushAsync(CancellationToken cancellationToken = default)
    {
        _producer.Flush(cancellationToken);
        return Task.FromResult(Interlocked.Exchange(ref _failedCount, 0));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }

    private void HandleDeliveryReport(DeliveryReport<string, string> report)
    {
        if (report.Error.IsError)
        {
            Interlocked.Increment(ref _failedCount);
            _logger.LogError(
                "Kafka delivery failed for {Topic}/{Key}: {Error}",
                report.Topic,
                report.Message.Key,
                report.Error.Reason);
        }
    }
}
