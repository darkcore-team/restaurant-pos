using Microsoft.EntityFrameworkCore;
using TablesService.Domain.Entities;
using TablesService.Domain.Repositories;
using TablesService.Infrastructure.Data;

namespace TablesService.Infrastructure.Repositories;

public sealed class TableAssignmentRepository(TablesServiceDbContext db) : ITableAssignmentRepository
{
    public async Task<TableAssignment?> GetActiveByTableAsync(long tableId, CancellationToken ct = default)
        => await db.TableAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.TableId == tableId && a.ReleasedAt == null, ct);

    public async Task<TableAssignment?> GetActiveByWaiterAsync(long waiterId, CancellationToken ct = default)
        => await db.TableAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.WaiterId == waiterId && a.ReleasedAt == null, ct);

    public async Task CreateAsync(TableAssignment assignment, CancellationToken ct = default)
    {
        db.TableAssignments.Add(assignment);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TableAssignment assignment, CancellationToken ct = default)
    {
        db.TableAssignments.Update(assignment);
        await db.SaveChangesAsync(ct);
    }
}