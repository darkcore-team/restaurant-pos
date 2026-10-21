using Microsoft.EntityFrameworkCore;
using ReportingService.Domain.Entities;
using RestaurantPos.Shared.Persistence;

namespace ReportingService.Infrastructure.Data;

public sealed class ReportingServiceDbContext(DbContextOptions options) : TenantDbContext(options)
{
    protected override string Schema => "reporting";

    public DbSet<DailySales> DailySales => Set<DailySales>();
    public DbSet<DailyProductSales> DailyProductSales => Set<DailyProductSales>();
    public DbSet<DailyWaiterSales> DailyWaiterSales => Set<DailyWaiterSales>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingServiceDbContext).Assembly);
    }
}
