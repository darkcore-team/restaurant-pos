using Microsoft.EntityFrameworkCore;
using TablesService.Domain.Entities;
using TablesService.Domain.Repositories;
using TablesService.Infrastructure.Data;

namespace TablesService.Infrastructure.Repositories;

public sealed class ZoneRepository(TablesServiceDbContext db) : IZoneRepository
{
    public async Task<bool> ExistsAsync(long id, long restaurantId, long branchId, CancellationToken ct = default)
        => await db.Zones.AnyAsync(z => z.Id == id && z.RestaurantId == restaurantId && z.BranchId == branchId && z.IsActive, ct);

    public async Task<IReadOnlyList<Zone>> GetByBranchAsync(long restaurantId, long branchId, CancellationToken ct = default)
        => await db.Zones
            .AsNoTracking()
            .Where(z => z.RestaurantId == restaurantId && z.BranchId == branchId && z.IsActive)
            .OrderBy(z => z.Position)
            .ThenBy(z => z.Name)
            .ToListAsync(ct);

    public async Task<Zone?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default)
        => await db.Zones
            .FirstOrDefaultAsync(z => z.Id == id && z.RestaurantId == restaurantId && z.IsActive, ct);

    public async Task CreateAsync(Zone zone, CancellationToken ct = default)
    {
        db.Zones.Add(zone);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Zone zone, CancellationToken ct = default)
    {
        db.Zones.Update(zone);
        await db.SaveChangesAsync(ct);
    }
}
