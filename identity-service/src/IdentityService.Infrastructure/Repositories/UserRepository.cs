using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public sealed class UserRepository(IdentityServiceDbContext dbContext) : IUserRepository
{
    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct)
    {
        return dbContext.Users.AnyAsync(u => u.Username == username, ct);
    }

    public async Task<IReadOnlyList<Authority>> GetByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken ct)
    {
        return await dbContext.Authorities
            .AsNoTracking()
            .Where(a => codes.Contains(a.Code))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Branch>> GetByIdsAsync(IReadOnlyCollection<long> ids, CancellationToken ct)
    {
        return await dbContext.Branches
            .AsNoTracking()
            .Where(b => ids.Contains(b.Id))
            .ToListAsync(ct);
    }

    public async Task<long> CreateAsync(User user, CancellationToken ct)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);
        return user.Id;
    }
}