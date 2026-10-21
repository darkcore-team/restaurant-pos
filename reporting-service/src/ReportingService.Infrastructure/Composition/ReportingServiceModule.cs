using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReportingService.Application.Reporting;
using ReportingService.Infrastructure.Data;
using ReportingService.Infrastructure.Repositories;
using ReportingService.Infrastructure.Workers;
using RestaurantPos.Shared.Infrastructure.Authentication;
using RestaurantPos.Shared.Infrastructure.Health;
using RestaurantPos.Shared.Infrastructure.Messaging;
using RestaurantPos.Shared.Persistence;

namespace ReportingService.Infrastructure.Composition;

public static class ReportingServiceModule
{
    public static IServiceCollection AddReportingService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ReportingDb")
            ?? throw new InvalidOperationException("Connection string 'ReportingDb' not found");

        services.AddTenantHttpContext();
        services.AddTenantBackgroundContext();
        services.AddTenantDbContext<ReportingServiceDbContext>(
            connectionString, "reporting",
            npgsql => { },
            options => options.UseSnakeCaseNamingConvention());

        services.AddPosAuthentication(configuration);
        services.AddPosMessaging(configuration);

        services.AddScoped<IReportingReadRepository, ReportingReadRepository>();
        services.AddScoped<GetDailySalesUseCase>();
        services.AddScoped<GetTopProductsUseCase>();
        services.AddScoped<GetByWaiterUseCase>();
        services.AddScoped<GetTipsUseCase>();

        services.AddHostedService<ReportingConsumer>();

        services.AddPosHealthChecks("reporting-service");
        return services;
    }
}
