using RestaurantPos.Shared.Abstractions.Events;

namespace OrdersService.Domain.Events;

public sealed record OrderSentToKitchenEvent : DomainEvent
{
    public override string EventType => "orders.sent_to_kitchen";
    public long OrderId { get; init; }
    public string OrderNumber { get; init; } = null!;
    public long? TableId { get; init; }
    public string? StationCode { get; init; }
}
