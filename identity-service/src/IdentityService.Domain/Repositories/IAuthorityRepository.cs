using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface IAuthorityRepository
{
    Task<IReadOnlyList<Authority>> GetActiveAsync(CancellationToken ct);
}