using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface IStationRepository
{
    Task<IReadOnlyList<Station>> GetActiveStationsAsync(long restaurantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long id, long restaurantId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, long restaurantId, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<Station?> GetByIdAsync(long id, long restaurantId, CancellationToken cancellationToken = default);
    Task CreateAsync(Station station, CancellationToken cancellationToken = default);
    Task UpdateAsync(Station station, CancellationToken cancellationToken = default);
}
