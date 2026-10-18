using CatalogService.Domain.Constants;
using CatalogService.Infrastructure.Data;
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
        
        services.AddPosHealthChecks("catalog-service");

        return services;
    }
}