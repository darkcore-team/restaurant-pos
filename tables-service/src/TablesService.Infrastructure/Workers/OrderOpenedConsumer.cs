using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantPos.Shared.Abstractions.Events;
using RestaurantPos.Shared.Abstractions.Tenancy;
using RestaurantPos.Shared.Infrastructure.Messaging;
using TablesService.Domain.Constants;
using TablesService.Domain.Repositories;
using TablesService.Infrastructure.Data;
using TablesService.Infrastructure.Repositories;

namespace TablesService.Infrastructure.Workers;

public sealed class OrderOpenedConsumer(
    RabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<OrderOpenedConsumer> logger)
    : RabbitMqConsumer(connectionProvider, options, logger)
{
    protected override string ConsumerName => "tables";
    protected override IReadOnlyList<string> BindingPatterns => ["orders.order_opened"];

    protected override async Task HandleAsync(EventEnvelope envelope, CancellationToken ct)
    {
        if (envelope.RestaurantId is null) return;

        using var scope = scopeFactory.CreateScope();
        var setter = scope.ServiceProvider.GetRequiredService<ITenantContextSetter>();
        setter.SetTenant(envelope.RestaurantId.Value, envelope.BranchId);
        try
        {
            var payload = JsonSerializer.Deserialize<OrderOpenedPayload>(envelope.Data.GetRawText(), PosJson.Options);
            if (payload is null) return;
            if (payload.TableId is null) return;

            var tableRepo = scope.ServiceProvider.GetRequiredService<ITableRepository>();
            var table = await tableRepo.GetByIdAsync(payload.TableId.Value, envelope.RestaurantId.Value, ct);
            if (table is null) return;
            
            if (table.Status == TableStatus.Free)
            {
                table.Status = TableStatus.Occupied;
                table.UpdatedAt = DateTimeOffset.UtcNow;
                await tableRepo.UpdateAsync(table, ct);
            }
        }
        finally
        {
            setter.Clear();
        }
    }
}

public sealed record OrderOpenedPayload(long OrderId, string OrderNumber, long? TableId, long? OpenedBy);