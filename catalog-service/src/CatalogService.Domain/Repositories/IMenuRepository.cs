using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface IMenuRepository
{
    Task<IReadOnlyList<Category>> GetMenuByBranchAsync(long branchId, CancellationToken cancellationToken = default);
}