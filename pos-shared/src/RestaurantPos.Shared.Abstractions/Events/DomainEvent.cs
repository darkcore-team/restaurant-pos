namespace RestaurantPos.Shared.Abstractions.Events;

public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    public long? RestaurantId { get; init; }
    public long? BranchId { get; init; }
    public abstract string EventType { get; }
}

public sealed record DomainEventEnvelope<T>(T Event, Guid EventId, DateTimeOffset OccurredOn) where T : DomainEvent
{
    public static DomainEventEnvelope<T> From(T e) => new(e, e.EventId, e.OccurredOn);
}