using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class TaxRepository(CatalogServiceDbContext db) : ITaxRepository
{
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
        => await db.Taxes.AnyAsync(t => t.Id == id && t.Status == RecordStatus.Active, cancellationToken);

    public async Task<IReadOnlyList<Tax>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await db.Taxes
            .AsNoTracking()
            .Where(t => t.Status == RecordStatus.Active)
            .OrderBy(t => t.Code)
            .ToListAsync(cancellationToken);
}
