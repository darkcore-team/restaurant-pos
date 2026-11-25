using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface IOptionGroupRepository
{
    Task<OptionGroup?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<OptionGroup?> GetByIdWithOptionsAsync(long id, long restaurantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OptionGroup>> GetByRestaurantAsync(long restaurantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long id, long restaurantId, CancellationToken cancellationToken = default);
    Task<int> CountExistingAsync(IEnumerable<long> ids, long restaurantId, CancellationToken cancellationToken = default);
    Task CreateAsync(OptionGroup group, CancellationToken cancellationToken = default);
    Task UpdateAsync(OptionGroup group, CancellationToken cancellationToken = default);
}
