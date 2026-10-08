namespace RestaurantPos.Shared.Persistence.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";
    
    public int BatchSize { get; set; } = 50;
    public int MaxAttempts { get; set; } = 5;
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
}
