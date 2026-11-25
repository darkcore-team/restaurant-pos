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

    public async Task<Tax?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await db.Taxes.FirstOrDefaultAsync(t => t.Id == id && t.Status == RecordStatus.Active, cancellationToken);

    public async Task<long> GetDefaultCountryIdAsync(CancellationToken cancellationToken = default)
    {
        var countryId = await db.Countries
            .AsNoTracking()
            .Select(c => (long?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return countryId ?? 1L;
    }

    public async Task CreateAsync(Tax tax, CancellationToken cancellationToken = default)
    {
        db.Taxes.Add(tax);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tax tax, CancellationToken cancellationToken = default)
    {
        db.Taxes.Update(tax);
        await db.SaveChangesAsync(cancellationToken);
    }
}
