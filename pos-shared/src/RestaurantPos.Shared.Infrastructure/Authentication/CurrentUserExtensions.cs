using System.Security.Claims;

namespace RestaurantPos.Shared.Infrastructure.Authentication;

public static class CurrentUserExtensions
{
    public static long? UserId(this ClaimsPrincipal user) => user.ReadGuid("sub");

    public static long? RestaurantId(this ClaimsPrincipal user) =>
        user.ReadGuid(PosClaims.RestaurantId);

    public static long? BranchId(this ClaimsPrincipal user) => user.ReadGuid(PosClaims.BranchId);

    public static string? DisplayName(this ClaimsPrincipal user) => user.FindFirst(PosClaims.DisplayName)?.Value;

    public static long RequireRestaurantId(this ClaimsPrincipal user) =>
        user.RestaurantId() ?? throw new InvalidOperationException(
            "The request has no restaurant_id claim. The endpoint is missing " +
            $"the {PosPolicies.TenantAccess} policy.");

    public static long RequireBranchId(this ClaimsPrincipal user) =>
        user.BranchId() ?? throw new InvalidOperationException(
            "The request has no branch_id claim. The endpoint is missing " +
            $"the {PosPolicies.TenantAccess} policy.");

    public static long RequireUserId(this ClaimsPrincipal user) =>
        user.UserId() ?? throw new InvalidOperationException(
            "The request has no subject claim.");
    
    private static long? ReadGuid(this ClaimsPrincipal user, string claim) =>
        long.TryParse(user.FindFirst(claim)?.Value, out var id) ? id : null;
}