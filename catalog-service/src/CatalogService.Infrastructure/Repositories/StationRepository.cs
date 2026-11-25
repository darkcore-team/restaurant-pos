using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class StationRepository(CatalogServiceDbContext db) : IStationRepository
{
    public async Task<IReadOnlyList<Station>> GetActiveStationsAsync(long restaurantId, CancellationToken cancellationToken = default)
        => await db.Stations
            .AsNoTracking()
            .Where(s => s.RestaurantId == restaurantId && s.Status == RecordStatus.Active)
            .OrderBy(s => s.Position)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(long id, long restaurantId, CancellationToken cancellationToken = default)
        => await db.Stations.AnyAsync(s => s.Id == id && s.RestaurantId == restaurantId && s.Status == RecordStatus.Active, cancellationToken);

    public async Task<bool> CodeExistsAsync(string code, long restaurantId, long? excludeId = null, CancellationToken cancellationToken = default)
        => await db.Stations.AnyAsync(s =>
            s.RestaurantId == restaurantId &&
            s.Code == code &&
            s.Status == RecordStatus.Active &&
            (!excludeId.HasValue || s.Id != excludeId.Value), cancellationToken);

    public async Task<Station?> GetByIdAsync(long id, long restaurantId, CancellationToken cancellationToken = default)
        => await db.Stations
            .FirstOrDefaultAsync(s => s.Id == id && s.RestaurantId == restaurantId && s.Status == RecordStatus.Active, cancellationToken);

    public async Task CreateAsync(Station station, CancellationToken cancellationToken = default)
    {
        db.Stations.Add(station);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Station station, CancellationToken cancellationToken = default)
    {
        db.Stations.Update(station);
        await db.SaveChangesAsync(cancellationToken);
    }
}
