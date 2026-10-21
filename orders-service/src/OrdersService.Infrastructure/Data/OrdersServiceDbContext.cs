using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using RestaurantPos.Shared.Persistence;
using RestaurantPos.Shared.Persistence.Outbox;

namespace OrdersService.Infrastructure.Data;

public sealed class OrdersServiceDbContext(DbContextOptions options) : TenantDbContext(options)
{
    protected override string Schema => "orders";

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemModifier> OrderItemModifiers => Set<OrderItemModifier>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();
    public DbSet<OrderPayment> OrderPayments => Set<OrderPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersServiceDbContext).Assembly);
        modelBuilder.ConfigureOutbox("outbox_messages");
    }
}