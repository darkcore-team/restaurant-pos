using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RestaurantPos.Shared.Abstractions.Events;

namespace RestaurantPos.Shared.Infrastructure.Messaging;

public abstract class RabbitMqConsumer(
    RabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options,
    ILogger logger) : BackgroundService
{
    private const string AttemptHeader = "x-pos-attempt";
    private readonly RabbitMqOptions _options = options.Value;
    
    protected abstract string ConsumerName { get; }
    protected abstract IReadOnlyList<string> BindingPatterns { get; }
    protected virtual int MaxAttempts => 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await connectionProvider.GetConnectionAsync(stoppingToken);
        
        await using var channel = await connection.CreateChannelAsync(
            options: new CreateChannelOptions(
                publisherConfirmationsEnabled: false,
                publisherConfirmationTrackingEnabled: false,
                consumerDispatchConcurrency: 1),
            cancellationToken: stoppingToken);
        
        var queue = RabbitMqTopology.QueueFor(ConsumerName);

        await RabbitMqTopology.DeclareExchangeAsync(channel, _options, stoppingToken);
        
        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = RabbitMqTopology.DeadLetterExchange,
            }, noWait: false, cancellationToken: stoppingToken);

        foreach (var pattern in BindingPatterns)
        {
            await channel.QueueBindAsync(
                queue: queue,
                exchange: _options.ExchangeName,
                routingKey: pattern,
                arguments: null,
                noWait: false,
                cancellationToken: stoppingToken);
        }
        
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _options.PrefetchCount,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        
        consumer.ReceivedAsync += async (_, args) => await HandleDeliveryAsync(channel, args, stoppingToken);
        
        await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumerTag: ConsumerName,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation(
            "Consumer {Consumer} listening on {Queue} bound to {Patterns}",
            ConsumerName, queue, string.Join(", ", BindingPatterns));

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    
    private async Task HandleDeliveryAsync(IChannel channel, BasicDeliverEventArgs args, CancellationToken ct)
    {
        var attempt = ReadAttempt(args.BasicProperties);

        try
        {
            var envelope = JsonSerializer.Deserialize<EventEnvelope>(args.Body.Span, PosJson.Options)
                ?? throw new InvalidOperationException("Message body is not a valid envelope.");

            await HandleAsync(envelope, ct);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Consumer {Consumer} failed on {RoutingKey} (attempt {Attempt}/{Max})",
                ConsumerName, args.RoutingKey, attempt + 1, MaxAttempts);

            if (attempt + 1 >= MaxAttempts)
            {
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: ct);
                return;
            }
            
            var properties = new BasicProperties
            {
                ContentType = args.BasicProperties.ContentType ?? "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = args.BasicProperties.MessageId,
                Type = args.BasicProperties.Type,
                Headers = new Dictionary<string, object?>
                {
                    [AttemptHeader] = attempt + 1
                }
            };

            await channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: args.RoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: args.Body,
                cancellationToken: ct);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: ct);
        }
    }
    
    protected abstract Task HandleAsync(EventEnvelope envelope, CancellationToken ct);
    
    private static int ReadAttempt(IReadOnlyBasicProperties? properties)
    {
        if (properties?.Headers is null || !properties.Headers.TryGetValue(AttemptHeader, out var raw))
            return 0;

        return raw switch
        {
            int i => i,
            long l => (int)l,
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out var parsed) => parsed,
            _ => 0
        };
    }
}