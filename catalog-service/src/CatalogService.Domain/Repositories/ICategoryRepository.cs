using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(long id, long restaurantId, long branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetByBranchAsync(long restaurantId, long branchId, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(long id, long restaurantId, CancellationToken cancellationToken = default);
    Task CreateAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
}
