using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Repositories;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Infrastructure.Repositories;

public class CashRegisterRepository(PaymentsServiceDbContext db) : ICashRegisterRepository
{
    public async Task<CashRegister?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default)
    {
        return await db.CashRegisters.FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurantId, ct);
    }

    public async Task<IReadOnlyList<CashRegister>> GetByBranchAsync(long branchId, long restaurantId, CancellationToken ct = default)
    {
        return await db.CashRegisters
            .AsNoTracking()
            .Where(c => c.BranchId == branchId && c.RestaurantId == restaurantId && c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
    }
}