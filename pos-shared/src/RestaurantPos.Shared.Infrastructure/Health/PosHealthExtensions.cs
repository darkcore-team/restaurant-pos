using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RestaurantPos.Shared.Infrastructure.Health;

public static class PosHealthExtensions
{
    public static IServiceCollection AddPosHealthChecks(this IServiceCollection services, string serviceName)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy($"{serviceName} is running"), tags: ["live"]);
        
        return services;
    }

    public static WebApplication MapPosHealth(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions() { Predicate = c => c.Tags.Contains("live") });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions() { Predicate = _ => true });

        return app;
    }
}