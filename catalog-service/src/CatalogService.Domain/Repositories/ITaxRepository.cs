using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface ITaxRepository
{
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
}