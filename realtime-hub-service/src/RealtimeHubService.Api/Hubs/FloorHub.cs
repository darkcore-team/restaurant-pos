using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace RealtimeHubService.Api.Hubs;

[Authorize]
public sealed class FloorHub : Hub
{
    public static string GroupName(long restaurantId, long? branchId)
        => branchId.HasValue ? $"floor:{restaurantId}:{branchId.Value}" : $"floor:{restaurantId}";

    public override async Task OnConnectedAsync()
    {
        var restaurantId = Context.User?.RestaurantId();
        var branchId = Context.User?.BranchId();

        if (restaurantId is null)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(restaurantId.Value, branchId));
        await base.OnConnectedAsync();
    }
}
