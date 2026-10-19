using TablesService.Domain.Entities;

namespace TablesService.Domain.Repositories;

public interface ITableRepository
{
    Task<DiningTable?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, long branchId, CancellationToken ct = default);
    Task CreateAsync(DiningTable table, CancellationToken ct = default);
    Task UpdateAsync(DiningTable table, CancellationToken ct = default);
}