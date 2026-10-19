using TablesService.Domain.Entities;

namespace TablesService.Domain.Repositories;

public interface ITableAssignmentRepository
{
    Task<TableAssignment?> GetActiveByTableAsync(long tableId, CancellationToken ct = default);
    Task<TableAssignment?> GetActiveByWaiterAsync(long waiterId, CancellationToken ct = default);
    Task CreateAsync(TableAssignment assignment, CancellationToken ct = default);
    Task UpdateAsync(TableAssignment assignment, CancellationToken ct = default);
}