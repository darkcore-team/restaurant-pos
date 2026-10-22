using RestaurantPos.Shared.Abstractions.Events;

namespace OrdersService.Domain.Events;

public sealed record OrderOpenedEvent : DomainEvent
{
    public override string EventType => "orders.order_opened";
    public long OrderId { get; init; }
    public string OrderNumber { get; init; } = null!;
    public long? TableId { get; init; }
    public long? OpenedBy { get; init; }
}