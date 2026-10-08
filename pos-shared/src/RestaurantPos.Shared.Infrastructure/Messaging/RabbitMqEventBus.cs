using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RestaurantPos.Shared.Abstractions.Events;

namespace RestaurantPos.Shared.Infrastructure.Messaging;

public sealed class RabbitMqEventBus(
    RabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqEventBus> logger) : IEventBus, IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IChannel? _channel;

    public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : DomainEvent
        => PublishAsync(EventEnvelope.From(@event), ct);

    public async Task PublishAsync(EventEnvelope envelope, CancellationToken ct = default)
    {
        var connection = await connectionProvider.GetConnectionAsync(ct);
        await _gate.WaitAsync(ct);

        try
        {
            var channel = await GetChannelAsync(connection, ct);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope, PosJson.Options));

            var properties = new BasicProperties()
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = envelope.EventId.ToString(),
                Type = envelope.EventType,
                Timestamp = new AmqpTimestamp(envelope.OccurredOn.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: envelope.EventType,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IChannel> GetChannelAsync(IConnection connection, CancellationToken ct)
    {
        if (_channel is { IsOpen: true })
            return _channel;

        _channel = await connection.CreateChannelAsync(new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true), cancellationToken: ct);

        await RabbitMqTopology.DeclareExchangeAsync(_channel, _options, ct);
        
        logger.LogInformation("RabbitMQ channel opened on exchange {Exchange}", _options.ExchangeName);
        return _channel;
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            try { await _channel.CloseAsync(); } catch { /* already broken */ }
            await _channel.DisposeAsync();
        }
        _gate.Dispose();
    }
}