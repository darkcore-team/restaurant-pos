namespace RestaurantPos.Shared.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    
    public string ExchangeName { get; set; } = "pos.events";
    public ushort PrefetchCount { get; set; } = 20;
    
    public int StartupRetryCount { get; set; } = 10;
    public int StartupRetryDelaySeconds { get; set; } = 5;
    public int NetworkRecoveryIntervalSeconds { get; set; } = 10;
}
