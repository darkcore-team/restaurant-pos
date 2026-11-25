using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class PaymentMethodRepository(CatalogServiceDbContext db) : IPaymentMethodRepository
{
    public async Task<IReadOnlyList<PaymentMethod>> GetActiveAsync(CancellationToken ct = default)
    {
        return await db.PaymentMethods.AsNoTracking()
            .Where(p => p.Status == RecordStatus.Active)
            .OrderBy(p => p.Position)
            .ThenBy(p => p.Code)
            .ToListAsync(ct);
    }

    public async Task<PaymentMethod?> GetByIdAsync(long id, CancellationToken ct = default)
        => await db.PaymentMethods.FirstOrDefaultAsync(p => p.Id == id && p.Status == RecordStatus.Active, ct);

    public async Task CreateAsync(PaymentMethod paymentMethod, CancellationToken ct = default)
    {
        db.PaymentMethods.Add(paymentMethod);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PaymentMethod paymentMethod, CancellationToken ct = default)
    {
        db.PaymentMethods.Update(paymentMethod);
        await db.SaveChangesAsync(ct);
    }
}