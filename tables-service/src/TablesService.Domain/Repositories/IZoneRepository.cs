using TablesService.Domain.Entities;

namespace TablesService.Domain.Repositories;

public interface IZoneRepository
{
    Task<bool> ExistsAsync(long id, long restaurantId, long branchId, CancellationToken ct = default);
}