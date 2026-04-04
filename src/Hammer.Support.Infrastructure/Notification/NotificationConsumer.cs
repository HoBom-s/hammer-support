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

    /// <summary>
    /// Validates that a notification request has required fields.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <returns>True if the request is valid; otherwise, false.</returns>
    internal static bool IsValidRequest(NotificationRequest? request) =>
        request is not null
        && !string.IsNullOrWhiteSpace(request.TemplateKey)
        && !string.IsNullOrWhiteSpace(request.RecipientToken);

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
            EnableAutoCommit = false,
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

                await ProcessMessageAsync(consumer, result, stoppingToken);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
            }
        }

        consumer.Close();
    }

    private async Task ProcessMessageAsync(IConsumer<string, string> consumer, ConsumeResult<string, string> result, CancellationToken ct)
    {
        NotificationRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<NotificationRequest>(result.Message.Value, _jsonOptions);
        }
#pragma warning disable CA1031
        catch (JsonException ex)
#pragma warning restore CA1031
        {
            _logger.LogError(ex, "Failed to deserialize notification request: {Message}", result.Message.Value[..Math.Min(result.Message.Value.Length, 200)]);
            consumer.Commit(result);
            return;
        }

        if (!IsValidRequest(request))
        {
            _logger.LogWarning("Invalid notification request — missing TemplateKey or RecipientToken");
            consumer.Commit(result);
            return;
        }

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        INotificationOrchestrator orchestrator = scope.ServiceProvider.GetRequiredService<INotificationOrchestrator>();
        await orchestrator.ProcessAsync(request!, ct);

        consumer.Commit(result);
    }
}
