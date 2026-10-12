using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface IUserBranchRepository
{
    Task<IReadOnlyList<UserBranch>> GetBranchesForUserAsync(long userId, CancellationToken cancellationToken);
    Task<Branch?> GetBranchIdByTenantAsync(long branchId, long restaurantId, CancellationToken ct);
    Task<bool> IsAssignedToBranchAsync(long userId, long branchId, CancellationToken ct);
    Task<bool> UserHasAnyBranchAsync(long userId, CancellationToken ct);
    Task AssignToBranchAsync(UserBranch assigment, CancellationToken ct);
}