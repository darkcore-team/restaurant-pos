using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Configuration;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).UseIdentityByDefaultColumn();
        builder.Property(h => h.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(h => h.ChangedAt).HasDefaultValueSql("now()");
        builder.Property(h => h.Reason).HasColumnType("text");
        builder.Property(h => h.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(h => h.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(h => new { h.OrderId, h.ChangedAt }).HasDatabaseName("ix_osh_order").IsDescending(false, true);

        builder.HasOne(h => h.Order).WithMany(o => o.StatusHistory).HasForeignKey(h => h.OrderId);
    }
}