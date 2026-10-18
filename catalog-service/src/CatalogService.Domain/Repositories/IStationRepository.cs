using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface IStationRepository
{
    Task<IReadOnlyList<Station>> GetActiveStationsAsync(CancellationToken cancellationToken = default);
}