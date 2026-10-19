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
            .OrderBy(p => p.Code)
            .ToListAsync(ct);
    }
}