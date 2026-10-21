using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportingService.Domain.Entities;

namespace ReportingService.Infrastructure.Configuration;

public class DailyWaiterSalesConfiguration : IEntityTypeConfiguration<DailyWaiterSales>
{
    public void Configure(EntityTypeBuilder<DailyWaiterSales> builder)
    {
        builder.ToTable("daily_waiter_sales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Total).HasPrecision(14, 2);
        builder.Property(x => x.Tips).HasPrecision(14, 2);
        builder.HasIndex(x => new { x.RestaurantId, x.BranchId, x.BusinessDate, x.WaiterId }).IsUnique();
    }
}
