using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).UseIdentityByDefaultColumn();
        builder.Property(o => o.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(o => o.OrderNumber).HasMaxLength(30).IsRequired();
        builder.Property(o => o.OrderType).HasDefaultValue(OrderType.DineIn);
        builder.Property(o => o.Status).HasDefaultValue(OrderStatus.Open);
        builder.Property(o => o.ExchangeRate).HasPrecision(14, 6).HasDefaultValue(1m);
        builder.Property(o => o.CustomerName).HasMaxLength(160);
        builder.Property(o => o.CustomerPhone).HasMaxLength(32);
        builder.Property(o => o.Covers).HasDefaultValue((short)1);
        builder.Property(o => o.Subtotal).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(o => o.DiscountAmount).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(o => o.TaxAmount).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(o => o.Total).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(o => o.TipAmount).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(o => o.PaidAmount).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(o => o.BalanceDue).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(o => o.OpenedAt).HasDefaultValueSql("now()");
        builder.Property(o => o.Notes).HasColumnType("text");
        builder.Property(o => o.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(o => o.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(o => new { o.BranchId, o.RestaurantId, o.OpenedAt }).HasDatabaseName("ix_orders_branch_date");
        builder.HasIndex(o => new { o.OpenedBy, o.OpenedAt }).HasDatabaseName("ix_orders_waiter");
    }
}