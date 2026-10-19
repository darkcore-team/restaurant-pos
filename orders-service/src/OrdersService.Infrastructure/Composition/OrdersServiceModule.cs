using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantPos.Shared.Infrastructure.Authentication;
using RestaurantPos.Shared.Infrastructure.Health;
using RestaurantPos.Shared.Persistence;
using OrdersService.Application.Kds;
using OrdersService.Application.Orders;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Repositories;
using OrdersService.Infrastructure.Data;
using OrdersService.Infrastructure.Repositories;

namespace OrdersService.Infrastructure.Composition;

public static class OrdersServiceModule
{
    public static IServiceCollection AddOrdersService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrdersDb")
            ?? throw new InvalidOperationException("Connection string 'OrdersDb' not found");

        services.AddTenantHttpContext();
        services.AddTenantDbContext<OrdersServiceDbContext>(
            connectionString, "orders",
            npgsql =>
            {
                npgsql.MapEnum<RecordStatus>(enumName: "record_status", schemaName: "orders");
                npgsql.MapEnum<OrderStatus>(enumName: "order_status", schemaName: "orders");
                npgsql.MapEnum<OrderItemStatus>(enumName: "order_item_status", schemaName: "orders");
                npgsql.MapEnum<OrderType>(enumName: "order_type", schemaName: "orders");
            },
            options => options.UseSnakeCaseNamingConvention());

        services.AddPosAuthentication(configuration);

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<OpenOrderUseCase>();
        services.AddScoped<GetOrderUseCase>();
        services.AddScoped<AddItemsUseCase>();
        services.AddScoped<VoidItemUseCase>();
        services.AddScoped<SendToKitchenUseCase>();
        services.AddScoped<UpdateItemStatusUseCase>();
        services.AddScoped<GetKdsUseCase>();
        services.AddScoped<GetActiveOrdersUseCase>();
        services.AddScoped<RequestBillUseCase>();
        services.AddScoped<CancelOrderUseCase>();

        services.AddPosHealthChecks("orders-service");
        return services;
    }
}