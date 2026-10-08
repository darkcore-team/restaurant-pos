using RabbitMQ.Client;

namespace RestaurantPos.Shared.Infrastructure.Messaging;

public static class RabbitMqTopology
{
    public const string ExchangeType = "topic";
    public const string DeadLetterExchange = "pos.events.dlx";

    public static string QueueFor(string consumerName) => $"pos.{consumerName}.queue";

    public static async Task DeclareExchangeAsync(IChannel channel, RabbitMqOptions options, CancellationToken ct)
    {
        await channel.ExchangeDeclareAsync(
            exchange: options.ExchangeName,
            type: ExchangeType,
            durable: true,
            autoDelete: false,
            arguments: null,
            noWait: false,
            cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            exchange: DeadLetterExchange,
            type: ExchangeType,
            durable: true,
            autoDelete: false,
            arguments: null,
            noWait: false,
            cancellationToken: ct);
    }
}