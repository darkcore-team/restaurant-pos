using Microsoft.EntityFrameworkCore;
using TablesService.Domain.Constants;
using TablesService.Domain.Entities;
using TablesService.Domain.Repositories;
using TablesService.Infrastructure.Data;

namespace TablesService.Infrastructure.Repositories;

public sealed class ZoneRepository(TablesServiceDbContext db) : IZoneRepository
{
    public async Task<bool> ExistsAsync(long id, long restaurantId, long branchId, CancellationToken ct = default)
        => await db.Zones.AnyAsync(z => z.Id == id && z.RestaurantId == restaurantId && z.BranchId == branchId && z.IsActive, ct);
}