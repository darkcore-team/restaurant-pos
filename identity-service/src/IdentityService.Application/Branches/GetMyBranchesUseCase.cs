using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Branches;

public static class GetMyBranchesUseCase
{
    public static async Task<IReadOnlyList<BranchDto>> ExecuteAsync(
        IUserBranchRepository repository,
        long userId,
        CancellationToken cancellationToken)
    {
        var userBranches = await repository.GetBranchesForUserAsync(userId, cancellationToken);

        return userBranches
            .Select(ub => new BranchDto()
            {
                Id = ub.Branch.Id,
                PublicId = ub.Branch.PublicId,
                Code = ub.Branch.Code,
                Name = ub.Branch.Name,
                IsDefault = ub.Branch.IsDefault,
                RestaurantId = ub.Branch.RestaurantId
            })
            .ToList();
    }
}