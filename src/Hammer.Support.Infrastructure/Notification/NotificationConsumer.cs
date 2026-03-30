using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Confluent.Kafka;
using Hammer.Support.Application;
using Hammer.Support.Application.Abstractions;
using Hammer.Support.Application.Models;
using Hammer.Support.Infrastructure.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Support.Infrastructure.Notification;

/// <summary>
/// Background service that consumes notification requests from Kafka
/// and delegates processing to <see cref="INotificationOrchestrator"/>.
/// </summary>
[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Consumer loop, logging overhead negligible")]
public sealed class NotificationConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger<NotificationConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationConsumer"/> class.
    /// </summary>
    /// <param name="scopeFactory">Service scope factory for resolving scoped dependencies.</param>
    /// <param name="kafkaOptions">Kafka configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public NotificationConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<NotificationConsumer> logger)
    {
        ArgumentNullException.ThrowIfNull(kafkaOptions);

        _scopeFactory = scopeFactory;
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Yield to let the host finish startup before blocking on Consume().
        await Task.Yield();

        ConsumerConfig config = new()
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = _kafkaOptions.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
        };

        using IConsumer<string, string> consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(KafkaTopics.NotificationRequest);

        _logger.LogInformation(
            "Notification consumer started, subscribed to {Topic} (group: {GroupId})",
            KafkaTopics.NotificationRequest,
            _kafkaOptions.ConsumerGroupId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ConsumeResult<string, string> result = consumer.Consume(stoppingToken);
                if (result?.Message?.Value is null)
                    continue;

                await ProcessMessageAsync(result.Message.Value, stoppingToken);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
            }
        }

        consumer.Close();
    }

    private async Task ProcessMessageAsync(string messageValue, CancellationToken ct)
    {
        NotificationRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<NotificationRequest>(messageValue, _jsonOptions);
        }
#pragma warning disable CA1031
        catch (JsonException ex)
#pragma warning restore CA1031
        {
            _logger.LogError(ex, "Failed to deserialize notification request: {Message}", messageValue[..Math.Min(messageValue.Length, 200)]);
            return;
        }

        if (request is null)
        {
            _logger.LogWarning("Deserialized notification request is null");
            return;
        }

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        INotificationOrchestrator orchestrator = scope.ServiceProvider.GetRequiredService<INotificationOrchestrator>();
        await orchestrator.ProcessAsync(request, ct);
    }
}
