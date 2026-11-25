using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantPos.Shared.Abstractions.Events;
using RestaurantPos.Shared.Abstractions.Tenancy;
using RestaurantPos.Shared.Infrastructure.Messaging;
using TablesService.Domain.Constants;
using TablesService.Domain.Events;
using TablesService.Domain.Repositories;

namespace TablesService.Infrastructure.Workers;

public sealed class OrderOpenedConsumer(
    RabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    IEventBus eventBus,
    ILogger<OrderOpenedConsumer> logger)
    : RabbitMqConsumer(connectionProvider, options, logger)
{
    protected override string ConsumerName => "tables";
    protected override IReadOnlyList<string> BindingPatterns =>
    [
        "orders.order_opened",
        "orders.bill_requested",
        "orders.order_closed"
    ];

    protected override async Task HandleAsync(EventEnvelope envelope, CancellationToken ct)
    {
        if (envelope.RestaurantId is null) return;

        using var scope = scopeFactory.CreateScope();
        var setter = scope.ServiceProvider.GetRequiredService<ITenantContextSetter>();
        setter.SetTenant(envelope.RestaurantId.Value, envelope.BranchId);
        try
        {
            var payload = JsonSerializer.Deserialize<OrderTablePayload>(envelope.Data.GetRawText(), PosJson.Options);
            if (payload?.TableId is null) return;

            var tableRepo = scope.ServiceProvider.GetRequiredService<ITableRepository>();
            var table = await tableRepo.GetByIdAsync(payload.TableId.Value, envelope.RestaurantId.Value, ct);
            if (table is null) return;

            TableStatus? nextStatus = envelope.EventType switch
            {
                "orders.order_opened" when table.Status == TableStatus.Free => TableStatus.Occupied,
                "orders.bill_requested" when table.Status is TableStatus.Free or TableStatus.Occupied => TableStatus.Billing,
                "orders.order_closed" when table.Status != TableStatus.Free && table.Status != TableStatus.Dirty => TableStatus.Dirty,
                _ => null
            };

            if (nextStatus is null) return;

            table.Status = nextStatus.Value;
            table.UpdatedAt = DateTimeOffset.UtcNow;
            await tableRepo.UpdateAsync(table, ct);

            try
            {
                await eventBus.PublishAsync(new TableStatusChangedEvent
                {
                    RestaurantId = table.RestaurantId,
                    BranchId = table.BranchId,
                    TableId = table.Id,
                    TableCode = table.Code,
                    Status = table.Status.ToString()
                }, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to publish TableStatusChangedEvent for table {TableId}", table.Id);
            }
        }
        finally
        {
            setter.Clear();
        }
    }
}

public sealed record OrderTablePayload(long OrderId, string OrderNumber, long? TableId);
