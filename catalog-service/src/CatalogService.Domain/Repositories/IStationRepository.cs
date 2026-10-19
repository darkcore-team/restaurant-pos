using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface IStationRepository
{
    Task<IReadOnlyList<Station>> GetActiveStationsAsync(long restaurantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long id, long restaurantId, CancellationToken cancellationToken = default);
}