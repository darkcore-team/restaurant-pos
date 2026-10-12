using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Users;

public static class CreateUserUseCase
{
    public static async Task<CreateUserOutcome> ExecuteAsync(
        IUserRepository repository,
        IPasswordHasher<User> passwordHasher,
        long actorId,
        CreateUserRequest request,
        CancellationToken ct)
    {
        if (await repository.UsernameExistsAsync(request.Username, ct))
        {
            return new CreateUserOutcome() { ErrorCode = CreateUserErrors.UsernameTaken };
        }

        var codes = request.AuthorityCodes.Distinct().ToList();
        var authorities = await repository.GetByCodesAsync(codes, ct);
        
        if (authorities.Count != codes.Count)
        {
            return new CreateUserOutcome() { ErrorCode = CreateUserErrors.UnknownAuthority };
        }

        var branchIds = request.BranchIds.Distinct().ToList();
        var branches = await repository.GetByIdsAsync(branchIds, ct);

        if (branches.Count != branchIds.Count)
        {
            return new CreateUserOutcome() { ErrorCode = CreateUserErrors.UnknownBranch };
        }

        var user = new User()
        {
            Person = new Person()
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim()
            },
            Username = request.Username.Trim(),
            UserAuthorities = authorities.Select(a => new UserAuthority()
            {
                AuthorityId = a.Id,
                GrantedBy = actorId
            }).ToList(),
            UserBranches = branches.Select(b => new UserBranch()
            {
                BranchId = b.Id,
                CreatedBy = actorId
            }).ToList()
        };
        
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        var userId = await repository.CreateAsync(user, ct);
        return new CreateUserOutcome() { Succeeded = true, UserId = userId };
    }
}