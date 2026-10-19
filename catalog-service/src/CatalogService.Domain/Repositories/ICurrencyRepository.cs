using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface ICurrencyRepository
{
    Task<IReadOnlyList<Currency>> GetActiveAsync(CancellationToken ct = default);
}