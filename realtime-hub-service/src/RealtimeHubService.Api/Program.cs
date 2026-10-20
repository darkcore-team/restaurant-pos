using Microsoft.AspNetCore.Authentication.JwtBearer;
using RealtimeHubService.Api.Hubs;
using RealtimeHubService.Api.Workers;
using RestaurantPos.Shared.Infrastructure.Authentication;
using RestaurantPos.Shared.Infrastructure.Health;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPosAuthentication(builder.Configuration);

builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events ??= new JwtBearerEvents();
    var previous = options.Events.OnMessageReceived;
    options.Events.OnMessageReceived = async context =>
    {
        if (previous is not null) await previous(context);
        if (string.IsNullOrEmpty(context.Token))
        {
            var accessToken = context.Request.Query["access_token"].ToString();
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                context.Token = accessToken;
        }
    };
});

builder.Services.AddSignalR();
builder.Services.AddHostedService<RabbitMqConsumer>();
builder.Services.AddPosHealthChecks("realtime-hub-service");

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<KdsHub>("/hubs/kds");
app.MapHealthChecks("/health/live");

app.Run();