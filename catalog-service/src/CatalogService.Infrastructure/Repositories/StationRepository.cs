using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class StationRepository(CatalogServiceDbContext db) : IStationRepository
{
    public async Task<IReadOnlyList<Station>> GetActiveStationsAsync(CancellationToken cancellationToken = default)
    {
        return await db.Stations
            .AsNoTracking()
            .Where(s => s.Status == RecordStatus.Active)
            .OrderBy(s => s.Position)
            .ToListAsync(cancellationToken);
    }
}