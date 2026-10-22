using OrdersService.Api.Middleware;
using OrdersService.Infrastructure.Composition;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using RestaurantPos.Shared.Infrastructure.Authentication;
using RestaurantPos.Shared.Infrastructure.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOrdersService(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OrderRead",
        policy => policy.RequireRole("ADMIN", "OWNER", "MANAGER", "WAITER", "HOST", "CASHIER"));
});
builder.Services.AddPosAntiForgery();
builder.Services.AddControllers();

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UsePosLocalization();
app.UseCookieToBearer();
app.UsePosCsrf();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions() { Predicate = check => check.Tags.Contains("live") });
app.MapControllers();

app.Run();