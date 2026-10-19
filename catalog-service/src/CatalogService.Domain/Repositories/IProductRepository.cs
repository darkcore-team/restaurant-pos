using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default);
    Task CreateAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(long id, long restaurantId, long actorId, CancellationToken ct = default);
}