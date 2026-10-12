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

    public async Task<Branch?> GetBranchIdByTenantAsync(long branchId, long restaurantId, CancellationToken ct)
    {
        return await dbContext.Branches.AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == branchId && b.RestaurantId == restaurantId, ct);
    }

    public async Task<bool> IsAssignedToBranchAsync(long userId, long branchId, CancellationToken ct)
    {
        return await dbContext.UserBranches.AnyAsync(ub => ub.UserId == userId && ub.BranchId == branchId, ct);
    }

    public async Task<bool> UserHasAnyBranchAsync(long userId, CancellationToken ct)
    {
        return await dbContext.UserBranches.AnyAsync(ub => ub.UserId == userId, ct);
    }

    public async Task AssignToBranchAsync(UserBranch assigment, CancellationToken ct)
    {
        dbContext.UserBranches.Add(assigment);
        await dbContext.SaveChangesAsync(ct);
    }
}