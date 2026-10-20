using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.Configuration;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseIdentityByDefaultColumn();
        builder.Property(p => p.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.MethodCode).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Amount).HasPrecision(12, 2);
        builder.Property(p => p.AmountTendered).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(p => p.ChangeGiven).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(p => p.TipAmount).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(p => p.Reference).HasMaxLength(120);
        builder.Property(p => p.Status).HasDefaultValue(PaymentStatus.Completed);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("now()");
        builder.Property(p => p.DeletedAt).HasColumnType("timestamp with time zone");

        builder.ToTable(t => t.HasCheckConstraint("ck_payment_change", "change_given = amount_tendered - amount"));
    }
}