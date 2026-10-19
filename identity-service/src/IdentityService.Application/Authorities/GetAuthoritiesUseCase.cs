using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Authorities;

public static class GetAuthoritiesUseCase
{
    public static async Task<IReadOnlyList<AuthorityDto>> ExecuteAsync(
        IAuthorityRepository repository,
        CancellationToken ct)
    {
        var authorities = await repository.GetActiveAsync(ct);
        return authorities
            .Select(a => new AuthorityDto()
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Description = a.Description
            })
            .ToList();
    }
}