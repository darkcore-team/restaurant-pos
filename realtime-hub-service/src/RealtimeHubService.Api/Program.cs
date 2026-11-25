using RealtimeHubService.Api.Hubs;
using RealtimeHubService.Api.Workers;
using RestaurantPos.Shared.Infrastructure.Authentication;
using RestaurantPos.Shared.Infrastructure.Health;
using RestaurantPos.Shared.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPosAuthentication(builder.Configuration);
builder.Services.AddPosAntiForgery();
builder.Services.AddPosMessaging(builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddHostedService<RealtimeEventsConsumer>();
builder.Services.AddPosHealthChecks("realtime-hub-service");

var app = builder.Build();

app.UseCookieToBearer();
app.UseAuthentication();
app.UsePosCsrf();
app.UseAuthorization();

app.MapHub<KdsHub>("/hubs/kds");
app.MapHub<FloorHub>("/hubs/floor");
app.MapHealthChecks("/health/live");

app.Run();
