using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantPos.Shared.Persistence.Outbox;

public static class OutboxServiceExtensions
{
    public static IServiceCollection AddOutboxPublisher<TContext>(
        this IServiceCollection services, IConfiguration configuration) where TContext : DbContext
    {
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.AddHostedService<OutboxPublisher<TContext>>();
        return services;
    }
}