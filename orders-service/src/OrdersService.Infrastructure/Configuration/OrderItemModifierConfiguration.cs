using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Configuration;

public class OrderItemModifierConfiguration : IEntityTypeConfiguration<OrderItemModifier>
{
    public void Configure(EntityTypeBuilder<OrderItemModifier> builder)
    {
        builder.ToTable("order_item_modifiers");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).UseIdentityByDefaultColumn();
        builder.Property(m => m.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.Name).HasMaxLength(120).IsRequired();
        builder.Property(m => m.PriceDelta).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(m => m.Notes).HasColumnType("text");
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(m => m.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(m => m.OrderItemId).HasDatabaseName("ix_oim_item");

        builder.HasOne(m => m.OrderItem).WithMany(i => i.Modifiers).HasForeignKey(m => m.OrderItemId);
    }
}