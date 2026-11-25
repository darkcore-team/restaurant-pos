using RestaurantPos.Shared.Abstractions.Events;

namespace TablesService.Domain.Events;

public sealed record TableStatusChangedEvent : DomainEvent
{
    public override string EventType => "tables.table_status_changed";
    public long TableId { get; init; }
    public string TableCode { get; init; } = null!;
    public string Status { get; init; } = null!;
}
