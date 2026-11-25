using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Repositories;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Infrastructure.Repositories;

public class CashRegisterRepository(PaymentsServiceDbContext db) : ICashRegisterRepository
{
    public async Task<CashRegister?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default)
    {
        return await db.CashRegisters.FirstOrDefaultAsync(
            c => c.Id == id && c.RestaurantId == restaurantId && c.IsActive, ct);
    }

    public async Task<IReadOnlyList<CashRegister>> GetByBranchAsync(long branchId, long restaurantId, CancellationToken ct = default)
    {
        return await db.CashRegisters
            .AsNoTracking()
            .Where(c => c.BranchId == branchId && c.RestaurantId == restaurantId && c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
    }

    public Task<bool> CodeExistsAsync(string code, long branchId, long restaurantId, long? excludeId, CancellationToken ct = default)
    {
        return db.CashRegisters.AnyAsync(
            c => c.BranchId == branchId
                 && c.RestaurantId == restaurantId
                 && c.IsActive
                 && c.Code == code
                 && (!excludeId.HasValue || c.Id != excludeId.Value),
            ct);
    }

    public async Task CreateAsync(CashRegister cashRegister, CancellationToken ct = default)
    {
        db.CashRegisters.Add(cashRegister);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CashRegister cashRegister, CancellationToken ct = default)
    {
        db.CashRegisters.Update(cashRegister);
        await db.SaveChangesAsync(ct);
    }
}