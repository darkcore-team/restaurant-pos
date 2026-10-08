using Microsoft.AspNetCore.Http;

namespace RestaurantPos.Shared.Abstractions.Tenancy;

public sealed class ScopedTenantContext(IHttpContextAccessor accessor) : ITenantContext
{
    public long? RestaurantId => Read("restaurant_id");
    public long? BranchId => Read("branch_id");

    private long? Read(string claim) =>
        long.TryParse(accessor.HttpContext?.User.FindFirst(claim)?.Value, out var id) ? id : null;
}