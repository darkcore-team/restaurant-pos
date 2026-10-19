using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RealtimeHubService.Api.Hubs;

[Authorize]
public class KdsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        var restaurantId = user?.FindFirst("restaurant_id")?.Value;
        var station = Context.GetHttpContext()?.Request.Query["station"].ToString();

        if (string.IsNullOrWhiteSpace(restaurantId) || string.IsNullOrWhiteSpace(station))
        {
            Context.Abort();
            return;
        }

        var group = $"{restaurantId}:station:{station}";
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinStation(string stationCode)
    {
        var restaurantId = Context.User?.FindFirst("restaurant_id")?.Value;
        if (string.IsNullOrWhiteSpace(restaurantId) || string.IsNullOrWhiteSpace(stationCode))
            throw new HubException("Missing restaurant_id or station");

        var group = $"{restaurantId}:station:{stationCode}";
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
    }
}