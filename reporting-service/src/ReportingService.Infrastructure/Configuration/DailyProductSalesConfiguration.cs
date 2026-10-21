using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportingService.Domain.Entities;

namespace ReportingService.Infrastructure.Configuration;

public class DailyProductSalesConfiguration : IEntityTypeConfiguration<DailyProductSales>
{
    public void Configure(EntityTypeBuilder<DailyProductSales> builder)
    {
        builder.ToTable("daily_product_sales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(12, 3);
        builder.Property(x => x.Total).HasPrecision(14, 2);
        builder.HasIndex(x => new { x.RestaurantId, x.BranchId, x.BusinessDate, x.ProductName }).IsUnique();
    }
}
