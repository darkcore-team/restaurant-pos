using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrdersService.Domain.Constants;
using OrdersService.Infrastructure.Data;

namespace OrdersService.Infrastructure.Migrations;

public sealed class OrdersServiceDbContextFactory : IDesignTimeDbContextFactory<OrdersServiceDbContext>
{
    public OrdersServiceDbContext CreateDbContext(string[] args)
    {
        var configuration = Host.CreateDefaultBuilder(args)
            .Build()
            .Services
            .GetRequiredService<IConfiguration>();

        var connectionString = configuration.GetConnectionString("OrdersDb")
            ?? Environment.GetEnvironmentVariable("ORDERS_DB_CONNECTION")
            ?? "Host=localhost;Database=orders_design";

        var options = new DbContextOptionsBuilder<OrdersServiceDbContext>()
            .UseNpgsql(connectionString, o =>
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "orders");
                o.MapEnum<RecordStatus>(enumName: "record_status", schemaName: "orders");
                o.MapEnum<OrderStatus>(enumName: "order_status", schemaName: "orders");
                o.MapEnum<OrderItemStatus>(enumName: "order_item_status", schemaName: "orders");
                o.MapEnum<OrderType>(enumName: "order_type", schemaName: "orders");
            })
            .UseSnakeCaseNamingConvention()
            .Options;

        return new OrdersServiceDbContext(options);
    }
}