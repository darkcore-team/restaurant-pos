using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class CurrencyRepository(CatalogServiceDbContext db) : ICurrencyRepository
{
    public async Task<IReadOnlyList<Currency>> GetActiveAsync(CancellationToken ct = default)
    {
        return await db.Currencies.AsNoTracking()
            .Where(c => c.Status == RecordStatus.Active)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
    }
}