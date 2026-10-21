using RestaurantPos.Shared.Abstractions.Events;

namespace OrdersService.Domain.Events;

public sealed record OrderClosedEvent : DomainEvent
{
    public override string EventType => "orders.order_closed";
    public long OrderId { get; init; }
    public string OrderNumber { get; init; } = null!;
    public long? OpenedBy { get; init; }
    public decimal Total { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TipAmount { get; init; }
    public DateTimeOffset ClosedAt { get; init; }
    public IReadOnlyList<OrderClosedItem> Items { get; init; } = [];
}

public sealed record OrderClosedItem(string Name, decimal Quantity, decimal UnitPrice, decimal LineTotal);
