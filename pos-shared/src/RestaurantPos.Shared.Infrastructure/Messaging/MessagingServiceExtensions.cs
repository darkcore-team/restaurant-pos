using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantPos.Shared.Abstractions.Events;

namespace RestaurantPos.Shared.Infrastructure.Messaging;

public static class MessagingServiceExtensions
{
    public static IServiceCollection AddPosMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<RabbitMqConnectionProvider>();
        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        return services;
    }
}