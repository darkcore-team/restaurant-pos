using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Users;

public static class GetUserUseCase
{
    public static async Task<IReadOnlyList<UserDto>> ExecuteAsync(
        IUserRepository repository,
        long restaurantId,
        CancellationToken ct)
    {
        var users = await repository.GetTenantUsersAsync(restaurantId, ct);

        return users
            .Select(u => new UserDto()
            {
                Id = u.Id,
                PublicId = u.PublicId,
                Username = u.Username,
                FirsName = u.Person.FirstName,
                LastName = u.Person.LastName,
                Status = u.Status.ToString(),
                Roles = u.UserAuthorities
                    .Select(ua => ua.Authority.Code)
                    .Order()
                    .ToList(),
                Branches = u.UserBranches
                    .Select(ub => new BranchRefDto()
                    {
                        Id = ub.Branch.Id,
                        Code = ub.Branch.Code,
                        Name = ub.Branch.Name
                    })
                    .OrderBy(b => b.Code)
                    .ToList()
            })
            .ToList();
    }
}