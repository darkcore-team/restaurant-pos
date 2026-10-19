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
}