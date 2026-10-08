namespace RestaurantPos.Shared.Abstractions.Tenancy;

public interface ITenantContext
{
    long? RestaurantId { get; }
    long? BranchId { get; }
}

public interface ITenantContextSetter : ITenantContext
{
    void SetTenant(long restaurantId, long? branchId = null);
    void Clear();
}