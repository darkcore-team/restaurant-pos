using RealtimeHubService.Api.Hubs;
using RealtimeHubService.Api.Workers;
using RestaurantPos.Shared.Infrastructure.Authentication;
using RestaurantPos.Shared.Infrastructure.Health;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPosAuthentication(builder.Configuration);
builder.Services.AddPosAntiForgery();

builder.Services.AddSignalR();
builder.Services.AddHostedService<RabbitMqConsumer>();
builder.Services.AddPosHealthChecks("realtime-hub-service");

var app = builder.Build();

app.UseCookieToBearer();
app.UsePosCsrf();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<KdsHub>("/hubs/kds");
app.MapHealthChecks("/health/live");

app.Run();
