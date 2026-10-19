using Microsoft.EntityFrameworkCore;
using TablesService.Domain.Constants;
using TablesService.Domain.Entities;
using TablesService.Domain.Repositories;
using TablesService.Infrastructure.Data;

namespace TablesService.Infrastructure.Repositories;

public sealed class TableRepository(TablesServiceDbContext db) : ITableRepository
{
    public async Task<DiningTable?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default)
        => await db.DiningTables
            .Include(t => t.Zone)
            .Include(t => t.Assignments.Where(a => a.ReleasedAt == null))
            .FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == restaurantId && t.IsActive, ct);

    public async Task<bool> CodeExistsAsync(string code, long branchId, CancellationToken ct = default)
        => await db.DiningTables.AnyAsync(t => t.Code == code && t.BranchId == branchId, ct);

    public async Task CreateAsync(DiningTable table, CancellationToken ct = default)
    {
        db.DiningTables.Add(table);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DiningTable table, CancellationToken ct = default)
    {
        db.DiningTables.Update(table);
        await db.SaveChangesAsync(ct);
    }
}