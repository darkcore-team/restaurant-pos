using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface ITaxRepository
{
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tax>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Tax?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> GetDefaultCountryIdAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(Tax tax, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tax tax, CancellationToken cancellationToken = default);
}
