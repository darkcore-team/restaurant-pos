using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface IStationRepository
{
    Task<IReadOnlyList<Station>> GetActiveStationsAsync(long restaurantId, CancellationToken cancellationToken = default);
}