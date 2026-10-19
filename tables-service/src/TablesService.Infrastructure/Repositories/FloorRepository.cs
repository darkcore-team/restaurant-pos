using Microsoft.EntityFrameworkCore;
using TablesService.Domain.Constants;
using TablesService.Domain.Entities;
using TablesService.Domain.Repositories;
using TablesService.Infrastructure.Data;

namespace TablesService.Infrastructure.Repositories;

public sealed class FloorRepository(TablesServiceDbContext db) : IFloorRepository
{
    public async Task<IReadOnlyList<Zone>> GetFloorAsync(long restaurantId, long branchId, CancellationToken ct = default)
        => await db.Zones
            .AsNoTracking()
            .Where(z => z.RestaurantId == restaurantId && z.BranchId == branchId && z.IsActive)
            .OrderBy(z => z.Position)
            .Include(z => z.Tables.Where(t => t.IsActive).OrderBy(t => t.Position))
                .ThenInclude(t => t.Assignments.Where(a => a.ReleasedAt == null))
            .ToListAsync(ct);
}