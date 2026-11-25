using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface IBranchRepository
{
    Task<IReadOnlyList<Branch>> GetByRestaurantAsync(long restaurantId, CancellationToken ct);
    Task<Branch?> GetByIdAsync(long id, long restaurantId, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, long restaurantId, long? excludeId, CancellationToken ct);
    Task<long> GetDefaultCityIdAsync(long restaurantId, CancellationToken ct);
    Task CreateAsync(Branch branch, CancellationToken ct);
    Task UpdateAsync(Branch branch, CancellationToken ct);
}
