using IdentityService.Infrastructure.Composition;
using IdentityService.Infrastructure.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenIddict.Validation.AspNetCore;
using RestaurantPos.Shared.Infrastructure.Authentication;
using IdentityService.Api.Middleware;
using RestaurantPos.Shared.Infrastructure.Localization;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddIdentityService(builder.Configuration);
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddPosAuthorizationPolicies();
builder.Services.AddControllers();

var app = builder.Build();

// Middlewares
app.UsePosLocalization();
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = check => check.Tags.Contains("live") });
app.MapControllers();

// Development seed
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<IdentityDevSeeder>().SeedAsync();
}

app.Run();