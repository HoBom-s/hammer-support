using System.Text.Json;
using Confluent.Kafka;
using FluentAssertions;
using Hammer.Support.Infrastructure.Logging;
using NSubstitute;
using Serilog.Events;
using Serilog.Parsing;

namespace Hammer.Support.Tests.Infrastructure.Logging;

public sealed class KafkaErrorSinkTests : IDisposable
{
    private readonly IProducer<string, string> _producer = Substitute.For<IProducer<string, string>>();
    private readonly KafkaErrorSink _sut;

    public KafkaErrorSinkTests()
    {
        _sut = new KafkaErrorSink(_producer);
    }

    public void Dispose()
    {
        _sut.Dispose();
        _producer.Dispose();
    }

    [Fact]
    public void Emit_ShouldSkip_WhenExceptionIsNull()
    {
        LogEvent logEvent = CreateLogEvent(LogEventLevel.Error, exception: null);

        _sut.Emit(logEvent);

        _producer.DidNotReceive().Produce(
            Arg.Any<string>(),
            Arg.Any<Message<string, string>>(),
            Arg.Any<Action<DeliveryReport<string, string>>>());
    }

    [Theory]
    [InlineData(LogEventLevel.Verbose)]
    [InlineData(LogEventLevel.Debug)]
    [InlineData(LogEventLevel.Information)]
    [InlineData(LogEventLevel.Warning)]
    public void Emit_ShouldSkip_WhenLevelIsBelowError(LogEventLevel level)
    {
        LogEvent logEvent = CreateLogEvent(level, new InvalidOperationException("test"));

        _sut.Emit(logEvent);

        _producer.DidNotReceive().Produce(
            Arg.Any<string>(),
            Arg.Any<Message<string, string>>(),
            Arg.Any<Action<DeliveryReport<string, string>>>());
    }

    [Fact]
    public void Emit_ShouldProduce_WhenErrorWithException()
    {
        LogEvent logEvent = CreateLogEvent(LogEventLevel.Error, new InvalidOperationException("something broke"));

        _sut.Emit(logEvent);

        _producer.Received(1).Produce(
            "service-error-log",
            Arg.Any<Message<string, string>>(),
            Arg.Any<Action<DeliveryReport<string, string>>>());
    }

    [Fact]
    public void Emit_ShouldProduce_WhenFatalWithException()
    {
        LogEvent logEvent = CreateLogEvent(LogEventLevel.Fatal, new InvalidOperationException("fatal error"));

        _sut.Emit(logEvent);

        _producer.Received(1).Produce(
            "service-error-log",
            Arg.Any<Message<string, string>>(),
            Arg.Any<Action<DeliveryReport<string, string>>>());
    }

    [Fact]
    public void Emit_ShouldProduceCorrectJsonSchema()
    {
        Message<string, string>? captured = null;
        _producer.When(p => p.Produce(
                Arg.Any<string>(),
                Arg.Any<Message<string, string>>(),
                Arg.Any<Action<DeliveryReport<string, string>>>()))
            .Do(ci => captured = ci.Arg<Message<string, string>>());

        List<LogEventProperty> properties =
        [
            new("TraceId", new ScalarValue("trace-123")),
            new("RequestPath", new ScalarValue("/api/collections/kamco")),
            new("RequestMethod", new ScalarValue("POST")),
        ];
        InvalidOperationException exception = new("connection refused");
        LogEvent logEvent = CreateLogEvent(LogEventLevel.Error, exception, properties);

        _sut.Emit(logEvent);

        captured.Should().NotBeNull();
        captured!.Key.Should().Be("trace-123");

        using var doc = JsonDocument.Parse(captured.Value);
        JsonElement root = doc.RootElement;
        root.GetProperty("traceId").GetString().Should().Be("trace-123");
        root.GetProperty("source").GetString().Should().Be("hammer-support");
        root.GetProperty("level").GetString().Should().Be("Error");
        root.GetProperty("exceptionType").GetString().Should().Be("System.InvalidOperationException");
        root.GetProperty("message").GetString().Should().Be("connection refused");
        root.GetProperty("requestPath").GetString().Should().Be("/api/collections/kamco");
        root.GetProperty("requestMethod").GetString().Should().Be("POST");
        root.TryGetProperty("timestamp", out _).Should().BeTrue();
    }

    [Fact]
    public void Emit_ShouldUseEmptyString_WhenPropertiesAreMissing()
    {
        Message<string, string>? captured = null;
        _producer.When(p => p.Produce(
                Arg.Any<string>(),
                Arg.Any<Message<string, string>>(),
                Arg.Any<Action<DeliveryReport<string, string>>>()))
            .Do(ci => captured = ci.Arg<Message<string, string>>());

        LogEvent logEvent = CreateLogEvent(LogEventLevel.Error, new InvalidOperationException("test"));

        _sut.Emit(logEvent);

        captured.Should().NotBeNull();
        captured!.Key.Should().BeEmpty();

        using var doc = JsonDocument.Parse(captured.Value);
        JsonElement root = doc.RootElement;
        root.GetProperty("traceId").GetString().Should().BeEmpty();
        root.GetProperty("requestPath").GetString().Should().BeEmpty();
        root.GetProperty("requestMethod").GetString().Should().BeEmpty();
    }

    [Fact]
    public void Emit_ShouldNotThrow_WhenProduceFails()
    {
        _producer.When(p => p.Produce(
                Arg.Any<string>(),
                Arg.Any<Message<string, string>>(),
                Arg.Any<Action<DeliveryReport<string, string>>>()))
            .Do(_ => throw new ProduceException<string, string>(
                new Error(ErrorCode.Local_Transport),
                new DeliveryResult<string, string>()));

        LogEvent logEvent = CreateLogEvent(LogEventLevel.Error, new InvalidOperationException("test"));

        Action act = () => _sut.Emit(logEvent);

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldFlushAndDisposeProducer()
    {
        IProducer<string, string> producer = Substitute.For<IProducer<string, string>>();
        KafkaErrorSink sink = new(producer);

        sink.Dispose();

        producer.Received(1).Flush(Arg.Any<TimeSpan>());
        producer.Received(1).Dispose();
    }

    private static LogEvent CreateLogEvent(
        LogEventLevel level,
        Exception? exception,
        IEnumerable<LogEventProperty>? properties = null)
    {
        return new LogEvent(
            DateTimeOffset.UtcNow,
            level,
            exception,
            new MessageTemplate("Test message", []),
            properties ?? []);
    }
}
