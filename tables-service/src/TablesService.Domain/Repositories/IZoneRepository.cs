using TablesService.Domain.Entities;

namespace TablesService.Domain.Repositories;

public interface IZoneRepository
{
    Task<bool> ExistsAsync(long id, long restaurantId, long branchId, CancellationToken ct = default);
    Task<IReadOnlyList<Zone>> GetByBranchAsync(long restaurantId, long branchId, CancellationToken ct = default);
    Task<Zone?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default);
    Task CreateAsync(Zone zone, CancellationToken ct = default);
    Task UpdateAsync(Zone zone, CancellationToken ct = default);
}
