using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Repositories;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Infrastructure.Repositories;

public class CashSessionRepository(PaymentsServiceDbContext db) : ICashSessionRepository
{
    public async Task<CashSession?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default)
    {
        return await db.CashSessions
            .Include(s => s.CashRegister)
            .FirstOrDefaultAsync(s => s.Id == id && s.RestaurantId == restaurantId, ct);
    }

    public async Task<CashSession?> GetActiveAsync(long branchId, long restaurantId, CancellationToken ct = default)
    {
        return await db.CashSessions
            .AsNoTracking()
            .Include(s => s.CashRegister)
            .Where(s => s.BranchId == branchId && s.RestaurantId == restaurantId && s.Status == CashSessionStatus.Open)
            .OrderByDescending(s => s.OpenedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> HasOpenSessionForRegisterAsync(long cashRegisterId, long restaurantId, CancellationToken ct = default)
    {
        return await db.CashSessions.AnyAsync(s => s.CashRegisterId == cashRegisterId
            && s.RestaurantId == restaurantId
            && s.Status == CashSessionStatus.Open, ct);
    }

    public async Task CreateAsync(CashSession session, CancellationToken ct = default)
    {
        db.CashSessions.Add(session);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CashSession session, CancellationToken ct = default)
    {
        db.CashSessions.Update(session);
        await db.SaveChangesAsync(ct);
    }
}