namespace RestaurantPos.Shared.Infrastructure.Authentication;

public class PosPolicies
{
    public const string TenantAccess = "tenant_access";
    public const string WaiterOrAbove = "waiter_or_above";
    public const string ManagerOrAbove = "manager_or_above";
    public const string CashierOrAbove = "cashier_or_above";
    public const string AdminOnly = "admin_only";
}
