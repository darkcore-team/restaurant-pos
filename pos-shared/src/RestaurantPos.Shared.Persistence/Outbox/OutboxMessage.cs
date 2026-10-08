namespace RestaurantPos.Shared.Persistence.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty; // e.g. "orders.order_opened"
    public string Payload { get; set; } = "{}";
    public DateTimeOffset OccurredOn { get; set; }
    public DateTimeOffset? PublishedOn { get; set; }
    public DateTimeOffset? FailedOn { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public long? RestaurantId { get; set; }
    public long? BranchId { get; set; }
}
