using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using RealtimeHubService.Api.Hubs;
using RestaurantPos.Shared.Abstractions.Events;
using RestaurantPos.Shared.Infrastructure.Messaging;

namespace RealtimeHubService.Api.Workers;

public sealed class RealtimeEventsConsumer(
    RabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options,
    IHubContext<FloorHub> floorHub,
    IHubContext<KdsHub> kdsHub,
    ILogger<RealtimeEventsConsumer> logger)
    : RabbitMqConsumer(connectionProvider, options, logger)
{
    protected override string ConsumerName => "realtime";

    protected override IReadOnlyList<string> BindingPatterns =>
    [
        "orders.*",
        "payments.*",
        "tables.*"
    ];

    protected override async Task HandleAsync(EventEnvelope envelope, CancellationToken ct)
    {
        if (envelope.RestaurantId is null)
            return;

        var floorGroup = FloorHub.GroupName(envelope.RestaurantId.Value, envelope.BranchId);

        await floorHub.Clients.Group(floorGroup).SendAsync(
            "FloorUpdated",
            new
            {
                eventType = envelope.EventType,
                occurredOn = envelope.OccurredOn,
                data = envelope.Data
            },
            ct);

        if (envelope.Data.ValueKind == JsonValueKind.Object &&
            envelope.Data.TryGetProperty("stationCode", out var stationProp))
        {
            var stationCode = stationProp.GetString();
            if (!string.IsNullOrWhiteSpace(stationCode))
            {
                var kdsGroup = $"{envelope.RestaurantId.Value}:station:{stationCode}";
                await kdsHub.Clients.Group(kdsGroup).SendAsync(
                    "KdsItemUpdated",
                    envelope.Data,
                    ct);
            }
        }
    }
}
