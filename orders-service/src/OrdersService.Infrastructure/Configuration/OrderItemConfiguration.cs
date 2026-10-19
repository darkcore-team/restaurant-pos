using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Configuration;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.Id).UseIdentityByDefaultColumn();
        builder.Property(oi => oi.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(oi => oi.Status).HasDefaultValue(OrderItemStatus.Queued);
        builder.Property(oi => oi.Name).HasMaxLength(150).IsRequired();
        builder.Property(oi => oi.StationCode).HasMaxLength(20);
        builder.Property(oi => oi.Description).HasColumnType("text");
        builder.Property(oi => oi.UnitPrice).HasPrecision(12, 2);
        builder.Property(oi => oi.Quantity).HasPrecision(14, 4).HasDefaultValue(1m);
        builder.Property(oi => oi.ModifiersTotal).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(oi => oi.DiscountAmount).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(oi => oi.DiscountReason).HasMaxLength(120);
        builder.Property(oi => oi.TaxRate).HasPrecision(5, 4);
        builder.Property(oi => oi.TaxAmount).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(oi => oi.IsTaxExempt).HasDefaultValue(false);
        builder.Property(oi => oi.TaxInclusive).HasDefaultValue(false);
        builder.Property(oi => oi.LineTotal).HasComputedColumnSql("ROUND(((unit_price + modifiers_total) * quantity - discount_amount)::numeric, 2)", stored: true);
        builder.Property(oi => oi.Notes).HasColumnType("text");
        builder.Property(oi => oi.VoidReason).HasColumnType("text");
        builder.Property(oi => oi.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(oi => oi.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(oi => oi.OrderId).HasDatabaseName("ix_oi_order");
        builder.HasIndex(oi => new { oi.StationId, oi.Status, oi.OrderId }).HasDatabaseName("ix_oi_kds");

        builder.HasOne(oi => oi.Order).WithMany(o => o.Items).HasForeignKey(oi => oi.OrderId);
    }
}