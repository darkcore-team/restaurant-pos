using IdentityService.Domain.Constants;
using IdentityService.Infrastructure.Configuration;
using IdentityService.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantPos.Shared.Infrastructure.Health;
using RestaurantPos.Shared.Persistence;

namespace IdentityService.Infrastructure.Composition;

public static class IdentityServiceModule
{
    public static IServiceCollection AddIdentityService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityDb")
            ?? throw new InvalidOperationException("Connection string 'IdentityDb' not found");

        var identity = configuration.GetSection(IdentityOptions.SectionName);
        services.Configure<IdentityOptions>(identity);

        services.AddOptions<IdentityOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTenantHttpContext();
        services.AddTenantDbContext<IdentityServiceDbContext>(
            connectionString, "identity",
            npgsql => npgsql.MapEnum<RecordStatus>(enumName: "record_status", schemaName: "identity"),
            options => options.UseSnakeCaseNamingConvention());

        services.AddPosHealthChecks("identity-service");
        return services;
    }
}