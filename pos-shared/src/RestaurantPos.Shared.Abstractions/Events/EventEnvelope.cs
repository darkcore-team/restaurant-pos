using System.Text.Json;

namespace RestaurantPos.Shared.Abstractions.Events;

public record EventEnvelope(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredOn,
    long? RestaurantId,
    long? BranchId,
    JsonElement Data)
{
    public static EventEnvelope From<T>(T @event) where T : DomainEvent
    {
        var data = JsonSerializer.SerializeToElement(@event, @event.GetType(), PosJson.Options);
        return new EventEnvelope(
            @event.EventId,
            @event.EventType,
            @event.OccurredOn,
            @event.RestaurantId,
            @event.BranchId,
            data);
    }
}