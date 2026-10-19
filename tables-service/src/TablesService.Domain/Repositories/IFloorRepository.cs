using TablesService.Domain.Entities;

namespace TablesService.Domain.Repositories;

public interface IFloorRepository
{
    Task<IReadOnlyList<Zone>> GetFloorAsync(long restaurantId, long branchId, CancellationToken ct = default);
}