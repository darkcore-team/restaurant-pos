using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(long id, long restaurantId, long branchId, CancellationToken cancellationToken = default);
}