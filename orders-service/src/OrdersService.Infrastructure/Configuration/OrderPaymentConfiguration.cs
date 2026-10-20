using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Configuration;

public class OrderPaymentConfiguration : IEntityTypeConfiguration<OrderPayment>
{
    public void Configure(EntityTypeBuilder<OrderPayment> builder)
    {
        builder.ToTable("order_payments");
        builder.HasKey(p => p.PaymentPublicId);
        builder.Property(p => p.PaymentPublicId).ValueGeneratedNever();
        builder.Property(p => p.Status).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Amount).HasPrecision(12, 2);
        builder.Property(p => p.TipAmount).HasPrecision(12, 2);
        builder.Property(p => p.OccurredOn).HasColumnType("timestamp with time zone");
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        builder.HasIndex(p => p.OrderId);
    }
}
