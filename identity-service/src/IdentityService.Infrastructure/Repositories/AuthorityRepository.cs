using IdentityService.Domain.Constants;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public sealed class AuthorityRepository(IdentityServiceDbContext dbContext) : IAuthorityRepository
{
    public async Task<IReadOnlyList<Authority>> GetActiveAsync(CancellationToken ct)
    {
        return await dbContext.Authorities
            .AsNoTracking()
            .Where(a => a.Status == RecordStatus.Active)
            .OrderBy(a => a.Position)
            .ToListAsync(ct);
    }
}