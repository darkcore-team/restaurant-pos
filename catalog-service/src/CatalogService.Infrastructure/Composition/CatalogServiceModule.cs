using CatalogService.Application.Currencies;
using CatalogService.Application.Menu;
using CatalogService.Application.Items;
using CatalogService.Application.PaymentMethods;
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
using RestaurantPos.Shared.Infrastructure.Localization;
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
        services.AddPosLocalization();

        services.AddTransient<Microsoft.Extensions.Localization.IStringLocalizer>(sp =>
        {
            var factory = sp.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizerFactory>();
            var assemblyName = typeof(CatalogService.Application.Messages).Assembly.GetName().Name!;
            return factory.Create($"{assemblyName}.Resources.Messages", assemblyName);
        });

        services.AddScoped<IStationRepository, StationRepository>();
        services.AddScoped<GetStationsUseCase>();

        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<GetMenuUseCase>();
        services.AddScoped<CatalogDevSeeder>();

        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<GetCurrenciesUseCase>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<GetPaymentMethodsUseCase>();

services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<ICategoryRepository, CategoryRepository>();
services.AddScoped<ITaxRepository, TaxRepository>();
services.AddScoped<IOptionGroupRepository, OptionGroupRepository>();
services.AddScoped<CreateItemUseCase>();
services.AddScoped<UpdateItemUseCase>();
services.AddScoped<DeleteItemUseCase>();
        
        services.AddPosHealthChecks("catalog-service");

        return services;
    }
}