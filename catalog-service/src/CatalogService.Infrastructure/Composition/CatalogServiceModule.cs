using CatalogService.Application.Menu;
using CatalogService.Domain.Constants;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Repositories;
using CatalogService.Application.Stations;
using CatalogService.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantPos.Shared.Infrastructure.Authentication;
using RestaurantPos.Shared.Infrastructure.Health;
using RestaurantPos.Shared.Persistence;

namespace CatalogService.Infrastructure.Composition;

public static class CatalogServiceModule
{
    public static IServiceCollection AddCatalogService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CatalogDb")
            ?? throw new InvalidOperationException("Connection string 'CatalogDb' not found");

        // RLS and Multi-tenancy
        services.AddTenantHttpContext();
        services.AddTenantDbContext<CatalogServiceDbContext>(
            connectionString, "catalog",
            npgsql => npgsql.MapEnum<RecordStatus>(enumName: "record_status", schemaName: "catalog"),
            options => options.UseSnakeCaseNamingConvention());

        services.AddPosAuthentication(configuration);

        services.AddScoped<IStationRepository, StationRepository>();
        services.AddScoped<GetStationsUseCase>();

        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<GetMenuUseCase>();
        services.AddScoped<CatalogDevSeeder>();
        
        services.AddPosHealthChecks("catalog-service");

        return services;
    }
}