using IdentityService.Application.Branches;
using IdentityService.Application.Users;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Branches;

public static class AssignBranchUseCase
{
    public static async Task<AssignBranchOutcome> ExecuteAsync(
        IUserBranchRepository userBranchRepository,
        IUserRepository userRepository,
        long branchId,
        long userId,
        long restaurantId,
        long actorId,
        CancellationToken ct)
    {
        if (await userRepository.GetByIdAsync(userId, ct) is null)
            return new AssignBranchOutcome() { ErrorCode = AssignBranchErrors.UserNotFound };

        var branch = await userBranchRepository.GetBranchIdByTenantAsync(branchId, restaurantId, ct);
        if (branch is null)
            return new AssignBranchOutcome() { ErrorCode = AssignBranchErrors.BranchNotFound };
        
        if (await userBranchRepository.IsAssignedToBranchAsync(userId, branchId, ct))
            return new AssignBranchOutcome() { ErrorCode = AssignBranchErrors.AlreadyAssigned };

        var isPrimary = !await userBranchRepository.UserHasAnyBranchAsync(userId, ct);
        var assigment = new UserBranch()
        {
            UserId = userId,
            BranchId = branchId,
            IsPrimary = isPrimary,
            CreatedBy = actorId
        };

        await userBranchRepository.AssignToBranchAsync(assigment, ct);
        return new AssignBranchOutcome { Succeeded = true };
    }
}