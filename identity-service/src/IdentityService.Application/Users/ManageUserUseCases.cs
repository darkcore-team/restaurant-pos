using System.ComponentModel.DataAnnotations;
using IdentityService.Domain.Constants;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using RestaurantPos.Shared.Abstractions.Common;

namespace IdentityService.Application.Users;

public sealed class UpdateUserRequest
{
    [Required, MaxLength(100)] public string FirstName { get; set; } = null!;
    [Required, MaxLength(100)] public string LastName { get; set; } = null!;
    [Required] public string Status { get; set; } = "Active";
    [Required, MinLength(1)] public List<string> AuthorityCodes { get; set; } = [];
    [Required, MinLength(1)] public List<long> BranchIds { get; set; } = [];
}

public sealed class ResetPasswordRequest
{
    [Required, MinLength(6), MaxLength(128)] public string NewPassword { get; set; } = null!;
}

public sealed class UpdateUserUseCase(IUserRepository userRepository)
{
    public async Task<Answer> ExecuteAsync(
        long id, long restaurantId, long actorId, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetTrackedForTenantAsync(id, restaurantId, ct);
        if (user is null)
            return Answer.Fail("USER_NOT_FOUND", "Usuario no encontrado.");

        if (!Enum.TryParse<RecordStatus>(request.Status, ignoreCase: true, out var parsedStatus)
            || parsedStatus == RecordStatus.Deleted)
        {
            return Answer.Fail("INVALID_STATUS", "Estado inválido. Use Active o Inactive.");
        }

        var codes = request.AuthorityCodes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var authorities = await userRepository.GetByCodesAsync(codes, ct);
        if (authorities.Count != codes.Count)
            return Answer.Fail("UNKNOWN_AUTHORITY", "Uno o más roles no existen.");

        var branchIds = request.BranchIds.Distinct().ToList();
        var branches = await userRepository.GetByIdsAsync(branchIds, ct);
        if (branches.Count != branchIds.Count || branches.Any(b => b.RestaurantId != restaurantId))
            return Answer.Fail("UNKNOWN_BRANCH", "Una o más sucursales no pertenecen a este restaurante.");

        var now = DateTimeOffset.UtcNow;
        user.Person.FirstName = request.FirstName.Trim();
        user.Person.LastName = request.LastName.Trim();
        user.Person.UpdatedAt = now;
        user.Person.UpdatedBy = actorId;

        user.Status = parsedStatus;
        user.UpdatedAt = now;
        user.UpdatedBy = actorId;

        // Sync roles
        var desiredAuthorityIds = authorities.Select(a => a.Id).ToHashSet();
        var toRemoveAuth = user.UserAuthorities.Where(ua => !desiredAuthorityIds.Contains(ua.AuthorityId)).ToList();
        foreach (var item in toRemoveAuth)
            user.UserAuthorities.Remove(item);

        var existingAuthIds = user.UserAuthorities.Select(ua => ua.AuthorityId).ToHashSet();
        foreach (var authId in desiredAuthorityIds.Where(aid => !existingAuthIds.Contains(aid)))
        {
            user.UserAuthorities.Add(new UserAuthority
            {
                UserId = user.Id,
                AuthorityId = authId,
                GrantedBy = actorId,
            });
        }

        // Sync branches (only within this restaurant)
        var desiredBranchIds = branches.Select(b => b.Id).ToHashSet();
        var toRemoveBranches = user.UserBranches.Where(ub => !desiredBranchIds.Contains(ub.BranchId)).ToList();
        foreach (var item in toRemoveBranches)
            user.UserBranches.Remove(item);

        var existingBranchIds = user.UserBranches.Select(ub => ub.BranchId).ToHashSet();
        foreach (var branchId in desiredBranchIds.Where(bid => !existingBranchIds.Contains(bid)))
        {
            user.UserBranches.Add(new UserBranch
            {
                UserId = user.Id,
                BranchId = branchId,
                CreatedBy = actorId,
            });
        }

        await userRepository.UpdateAsync(user, ct);
        return Answer.Ok(message: "Usuario actualizado.");
    }
}

public sealed class ResetUserPasswordUseCase(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
{
    public async Task<Answer> ExecuteAsync(
        long id, long restaurantId, long actorId, ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetTrackedForTenantAsync(id, restaurantId, ct);
        if (user is null)
            return Answer.Fail("USER_NOT_FOUND", "Usuario no encontrado.");

        var now = DateTimeOffset.UtcNow;
        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        user.PasswordUpdatedAt = now;
        user.FailedCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = now;
        user.UpdatedBy = actorId;

        await userRepository.UpdateAsync(user, ct);
        return Answer.Ok(message: "Contraseña restablecida.");
    }
}

public sealed class UnlockUserUseCase(IUserRepository userRepository)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, long actorId, CancellationToken ct = default)
    {
        var user = await userRepository.GetTrackedForTenantAsync(id, restaurantId, ct);
        if (user is null)
            return Answer.Fail("USER_NOT_FOUND", "Usuario no encontrado.");

        user.FailedCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.UpdatedBy = actorId;

        await userRepository.UpdateAsync(user, ct);
        return Answer.Ok(message: "Usuario desbloqueado.");
    }
}

public sealed class ChangeOwnPasswordRequest
{
    [Required] public string CurrentPassword { get; set; } = null!;
    [Required, MinLength(6), MaxLength(128)] public string NewPassword { get; set; } = null!;
}

public sealed class ChangeOwnPasswordUseCase(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
{
    public async Task<Answer> ExecuteAsync(long userId, ChangeOwnPasswordRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetTrackedByIdAsync(userId, ct);
        if (user is null)
            return Answer.Fail("USER_NOT_FOUND", "Usuario no encontrado.");

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
            return Answer.Fail("INVALID_CURRENT_PASSWORD", "La contraseña actual es incorrecta.");

        var now = DateTimeOffset.UtcNow;
        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        user.PasswordUpdatedAt = now;
        user.MustChangePassword = false;
        user.UpdatedAt = now;
        user.UpdatedBy = userId;

        await userRepository.UpdateAsync(user, ct);
        return Answer.Ok(message: "Contraseña actualizada correctamente.");
    }
}
