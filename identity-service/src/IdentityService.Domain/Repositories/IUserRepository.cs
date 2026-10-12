using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface IUserRepository
{
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct);
    Task<IReadOnlyList<Authority>> GetByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken ct);
    Task<IReadOnlyList<Branch>> GetByIdsAsync(IReadOnlyCollection<long> ids, CancellationToken ct);
    Task<long> CreateAsync(User user, CancellationToken ct);
    Task<IReadOnlyList<User>> GetTenantUsersAsync(long restaurantId, CancellationToken ct);
    Task<User?> GetByIdAsync(long id, CancellationToken ct);
}
