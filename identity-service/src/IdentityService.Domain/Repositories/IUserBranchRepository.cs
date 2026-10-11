using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface IUserBranchRepository
{
    Task<IReadOnlyList<UserBranch>> GetBranchesForUserAsync(long userId, CancellationToken cancellationToken);
}