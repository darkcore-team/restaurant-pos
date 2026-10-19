using Microsoft.EntityFrameworkCore;
using TablesService.Domain.Entities;
using RestaurantPos.Shared.Persistence;

namespace TablesService.Infrastructure.Data;

public sealed class TablesServiceDbContext(DbContextOptions options) : TenantDbContext(options)
{
    protected override string Schema => "tables";

    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<DiningTable> DiningTables => Set<DiningTable>();
    public DbSet<TableAssignment> TableAssignments => Set<TableAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TablesServiceDbContext).Assembly);
    }
}