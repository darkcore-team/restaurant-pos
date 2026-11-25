using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace RealtimeHubService.Api.Hubs;

[Authorize]
public sealed class KdsHub : Hub
{
    public static string GroupName(long restaurantId, string stationCode)
        => $"{restaurantId}:station:{stationCode.Trim().ToUpperInvariant()}";

    public override async Task OnConnectedAsync()
    {
        var restaurantId = Context.User?.RestaurantId();
        if (restaurantId is null)
        {
            Context.Abort();
            return;
        }

        var station = Context.GetHttpContext()?.Request.Query["station"].ToString();
        if (!string.IsNullOrWhiteSpace(station))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(restaurantId.Value, station));
        }

        await base.OnConnectedAsync();
    }

    public async Task JoinStation(string stationCode)
    {
        var restaurantId = Context.User?.RestaurantId();
        if (restaurantId is null || string.IsNullOrWhiteSpace(stationCode))
            throw new HubException("Missing restaurant_id or station");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(restaurantId.Value, stationCode));
    }

    public async Task LeaveStation(string stationCode)
    {
        var restaurantId = Context.User?.RestaurantId();
        if (restaurantId is null || string.IsNullOrWhiteSpace(stationCode))
            return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(restaurantId.Value, stationCode));
    }
}
