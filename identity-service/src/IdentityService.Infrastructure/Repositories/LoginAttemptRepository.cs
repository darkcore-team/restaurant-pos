using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public sealed class LoginAttemptRepository(IdentityServiceDbContext dbContext) : ILoginAttemptRepository
{
    public async Task<IReadOnlyList<LoginAttempt>> GetRecentByRestaurantAsync(
        long restaurantId, int limit, CancellationToken ct)
    {
        var safeLimit = Math.Clamp(limit, 1, 200);

        var tenantUserIds = dbContext.UserBranches
            .Where(ub => ub.Branch.RestaurantId == restaurantId)
            .Select(ub => ub.UserId);

        var tenantUsernames = dbContext.Users
            .Where(u => tenantUserIds.Contains(u.Id))
            .Select(u => u.Username);

        return await dbContext.LoginAttempts
            .AsNoTracking()
            .Where(la => (la.UserId.HasValue && tenantUserIds.Contains(la.UserId.Value))
                         || tenantUsernames.Contains(la.UsernameTry))
            .OrderByDescending(la => la.CreatedAt)
            .Take(safeLimit)
            .ToListAsync(ct);
    }
}
