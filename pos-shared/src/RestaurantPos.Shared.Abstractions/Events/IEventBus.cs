namespace RestaurantPos.Shared.Abstractions.Events;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : DomainEvent;
    Task PublishAsync(EventEnvelope envelope, CancellationToken ct = default);
}
