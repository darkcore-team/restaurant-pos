using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantPos.Shared.Infrastructure.Authentication;
using RestaurantPos.Shared.Infrastructure.Health;
using RestaurantPos.Shared.Persistence;
using RestaurantPos.Shared.Infrastructure.Localization;
using TablesService.Application.Floor;
using TablesService.Application.Tables;
using TablesService.Domain.Constants;
using TablesService.Domain.Repositories;
using TablesService.Infrastructure.Data;
using TablesService.Infrastructure.Repositories;
using TablesService.Infrastructure.Seed;

namespace TablesService.Infrastructure.Composition;

public static class TablesServiceModule
{
    public static IServiceCollection AddTablesService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TablesDb")
            ?? throw new InvalidOperationException("Connection string 'TablesDb' not found");

        services.AddTenantHttpContext();
        services.AddTenantDbContext<TablesServiceDbContext>(
            connectionString, "tables",
            npgsql =>
            {
                npgsql.MapEnum<RecordStatus>(enumName: "record_status", schemaName: "tables");
                npgsql.MapEnum<TableStatus>(enumName: "table_status", schemaName: "tables");
            },
            options => options.UseSnakeCaseNamingConvention());

        services.AddPosAuthentication(configuration);
        services.AddPosLocalization();

        services.AddTransient<Microsoft.Extensions.Localization.IStringLocalizer>(sp =>
        {
            var factory = sp.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizerFactory>();
            var assemblyName = "TablesService.Application";
            return factory.Create($"{assemblyName}.Resources.Messages", assemblyName);
        });

        services.AddScoped<IFloorRepository, FloorRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<ITableAssignmentRepository, TableAssignmentRepository>();
        services.AddScoped<IZoneRepository, ZoneRepository>();

        services.AddScoped<GetFloorUseCase>();
        services.AddScoped<CreateTableUseCase>();
        services.AddScoped<UpdateTableStatusUseCase>();
        services.AddScoped<AssignTableUseCase>();
        services.AddScoped<ReleaseTableUseCase>();

        services.AddScoped<TablesDevSeeder>();

        services.AddPosHealthChecks("tables-service");
        return services;
    }
}