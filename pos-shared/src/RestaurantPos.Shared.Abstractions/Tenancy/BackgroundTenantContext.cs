namespace RestaurantPos.Shared.Abstractions.Tenancy;

public sealed class BackgroundTenantContext : ITenantContextSetter
{
    private static readonly AsyncLocal<Tenant?> Current = new();
    private sealed record Tenant(long RestaurantId, long? BranchId);
    public long? RestaurantId => Current.Value?.RestaurantId;
    public long? BranchId => Current.Value?.BranchId;

    public void SetTenant(long restaurantId, long? branchId = null)
        => Current.Value = new Tenant(restaurantId, branchId);
    
    public void Clear() => Current.Value = null;
}