using RestaurantPos.Shared.Abstractions.Events;

namespace OrdersService.Domain.Events;

public sealed record OrderItemStatusChangedEvent : DomainEvent
{
    public override string EventType => "orders.item_status_changed";
    public long OrderId { get; init; }
    public long OrderItemId { get; init; }
    public string? StationCode { get; init; }
    public string ItemStatus { get; init; } = null!;
    public string OrderStatus { get; init; } = null!;
}
