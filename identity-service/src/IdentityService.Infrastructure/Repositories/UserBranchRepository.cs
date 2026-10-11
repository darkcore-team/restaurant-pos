using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class UserBranchRepository(IdentityServiceDbContext dbContext) : IUserBranchRepository
{
    public async Task<IReadOnlyList<UserBranch>> GetBranchesForUserAsync(long userId, CancellationToken cancellationToken)
    {
        return await dbContext.UserBranches
            .AsNoTracking()
            .Include(ub => ub.Branch)
                .ThenInclude(b => b.Restaurant)
            .Where(ub => ub.UserId == userId)
            .OrderBy(ub => ub.Branch.Code)
            .ToListAsync(cancellationToken);
    }
}