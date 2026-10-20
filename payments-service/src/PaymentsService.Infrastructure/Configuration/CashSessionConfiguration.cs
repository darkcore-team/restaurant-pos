using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.Configuration;

public class CashSessionConfiguration : IEntityTypeConfiguration<CashSession>
{
    public void Configure(EntityTypeBuilder<CashSession> builder)
    {
        builder.ToTable("cash_sessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).UseIdentityByDefaultColumn();
        builder.Property(s => s.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.OpeningFloat).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(s => s.ExpectedAmount).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(s => s.CountedAmount).HasPrecision(12, 2);
        builder.Property(s => s.Difference).HasPrecision(12, 2);
        builder.Property(s => s.Status).HasDefaultValue(CashSessionStatus.Open);
        builder.Property(s => s.OpenedAt).HasDefaultValueSql("now()");
        builder.Property(s => s.Notes).HasColumnType("text");
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(s => s.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(s => s.CashRegister).WithMany().HasForeignKey(s => s.CashRegisterId);
    }
}