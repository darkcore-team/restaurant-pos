using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using RealtimeHubService.Api.Hubs;

namespace RealtimeHubService.Api.Workers;

public class RabbitMqConsumer(IHubContext<KdsHub> hub, IConfiguration config, ILogger<RabbitMqConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = config["RabbitMQ:Host"] ?? "localhost";
        var port = int.TryParse(config["RabbitMQ:Port"], out var p) ? p : 5672;
        var user = config["RabbitMQ:Username"] ?? "guest";
        var pass = config["RabbitMQ:Password"] ?? "guest";
        var vhost = config["RabbitMQ:VirtualHost"] ?? "/";

        IConnection? connection = null;
        IChannel? channel = null;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = user,
                Password = pass,
                VirtualHost = vhost
            };
            connection = await factory.CreateConnectionAsync(stoppingToken);
            channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync("realtime.events", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    logger.LogInformation("Received realtime event: {Body}", body);

                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;
                    var restaurantId = root.TryGetProperty("restaurantId", out var r) ? r.GetString() : null;
                    var stationCode = root.TryGetProperty("stationCode", out var s) ? s.GetString() : null;
                    var eventName = root.TryGetProperty("eventName", out var e) ? e.GetString() : "EventReceived";

                    if (!string.IsNullOrWhiteSpace(restaurantId) && !string.IsNullOrWhiteSpace(stationCode))
                    {
                        var group = $"{restaurantId}:station:{stationCode}";
                        await hub.Clients.Group(group).SendAsync(eventName!, body, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing realtime event");
                }
                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            };

            await channel.BasicConsumeAsync("realtime.events", autoAck: false, consumer, stoppingToken);
            logger.LogInformation("RabbitMQ consumer started on queue 'realtime.events'");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown normal
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RabbitMQ not available; realtime-hub running without event consumer. KDS will rely on HTTP polling");
        }
        finally
        {
            if (channel is not null) await channel.DisposeAsync();
            if (connection is not null) await connection.DisposeAsync();
        }
    }
}