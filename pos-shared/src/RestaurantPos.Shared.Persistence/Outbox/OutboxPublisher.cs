using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantPos.Shared.Abstractions.Events;

namespace RestaurantPos.Shared.Persistence.Outbox;

public sealed class OutboxPublisher<TContext>(
    IServiceScopeFactory scopeFactory,
    IEventBus eventBus,
    IOptions<OutboxOptions> options,
    ILogger<OutboxPublisher<TContext>> logger) : BackgroundService
    where TContext : DbContext
{
    private readonly OutboxOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox publisher started for {Context}", typeof(TContext).Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var published = await PublishBathAsync(stoppingToken);
                if (published == 0)
                {
                    await Task.Delay(_options.PollingInterval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox publisher failed, Retrying in {Interval}", _options.PollingInterval);
                await Task.Delay(_options.PollingInterval, stoppingToken);
            }
        }
    }

    private async Task<int> PublishBathAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();

        var batch = await db.Set<OutboxMessage>()
            .Where(m => m.PublishedOn == null && m.FailedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(_options.BatchSize)
            .ToListAsync(ct);

        var published = 0;

        foreach (var message in batch)
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<EventEnvelope>(message.Payload, PosJson.Options)
                               ?? throw new InvalidOperationException("Outbox payload is not a valid envelope.");

                await eventBus.PublishAsync(envelope, ct);

                message.PublishedOn = DateTimeOffset.UtcNow;
                message.LastError = null;
                published++;
            }
            catch (Exception ex)
            {
                message.Attempts++;
                message.LastError = Truncate(ex.Message);

                if (message.Attempts >= _options.MaxAttempts)
                {
                    message.FailedOn = DateTimeOffset.UtcNow;
                    logger.LogError(
                        ex, "Outbox message {Id} ({EventType}) failed {Attempts} times and was dropped", 
                        message.Id, message.EventType, message.Attempts);
                }
                else
                {
                    logger.LogWarning(
                        ex, "Outbox message {Id} ({EventType}) failed , attempt {Attempts}", 
                        message.Id, message.EventType, message.Attempts);
                }

                break;
            }
        }

        await db.SaveChangesAsync(ct);
        return published;
    }
    
    private static string Truncate(string value)
    {
        return value.Length <= 2000 ? value : value[..2000];
    }
}