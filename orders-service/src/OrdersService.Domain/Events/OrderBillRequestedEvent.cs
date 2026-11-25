using RestaurantPos.Shared.Abstractions.Events;

namespace OrdersService.Domain.Events;

public sealed record OrderBillRequestedEvent : DomainEvent
{
    public override string EventType => "orders.bill_requested";
    public long OrderId { get; init; }
    public string OrderNumber { get; init; } = null!;
    public long? TableId { get; init; }
}
