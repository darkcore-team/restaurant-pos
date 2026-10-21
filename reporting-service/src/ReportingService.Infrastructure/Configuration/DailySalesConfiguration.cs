using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportingService.Domain.Entities;

namespace ReportingService.Infrastructure.Configuration;

public class DailySalesConfiguration : IEntityTypeConfiguration<DailySales>
{
    public void Configure(EntityTypeBuilder<DailySales> builder)
    {
        builder.ToTable("daily_sales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.TotalSales).HasPrecision(14, 2);
        builder.Property(x => x.TotalDiscount).HasPrecision(14, 2);
        builder.Property(x => x.TotalTax).HasPrecision(14, 2);
        builder.Property(x => x.TotalTips).HasPrecision(14, 2);
        builder.Property(x => x.ByPaymentMethod).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        builder.HasIndex(x => new { x.RestaurantId, x.BranchId, x.BusinessDate }).IsUnique();
    }
}
