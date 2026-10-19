using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class StationRepository(CatalogServiceDbContext db) : IStationRepository
{
    public async Task<IReadOnlyList<Station>> GetActiveStationsAsync(long restaurantId, CancellationToken cancellationToken = default)
    {
        return await db.Stations
            .AsNoTracking()
            .Where(s => s.RestaurantId == restaurantId && s.Status == RecordStatus.Active)
            .OrderBy(s => s.Position)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(long id, long restaurantId, CancellationToken cancellationToken = default)
        => await db.Stations.AnyAsync(s => s.Id == id && s.RestaurantId == restaurantId && s.Status == RecordStatus.Active, cancellationToken);
}