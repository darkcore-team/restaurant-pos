using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Entities;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Infrastructure.Seed;

public sealed class PaymentsDevSeeder(PaymentsServiceDbContext db)
{
    private const long RestaurantId = 1;
    private const long BranchId = 1;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.CashRegisters.AnyAsync(c => c.RestaurantId == RestaurantId && c.BranchId == BranchId, ct)) return;

        db.CashRegisters.Add(new CashRegister
        {
            RestaurantId = RestaurantId,
            BranchId = BranchId,
            Code = "CAJA-1",
            Name = "Caja principal",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = 1,
            UpdatedAt = DateTimeOffset.UtcNow,
            UpdatedBy = 1
        });

        await db.SaveChangesAsync(ct);
    }
}
