using IdentityService.Application.Audit;
using IdentityService.Application.Branches;
using IdentityService.Application.Restaurants;
using IdentityService.Application.Users;
using IdentityService.Infrastructure.Composition;
using IdentityService.Infrastructure.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using OpenIddict.Validation.AspNetCore;
using RestaurantPos.Shared.Infrastructure.Authentication;
using IdentityService.Api.Middleware;
using RestaurantPos.Shared.Infrastructure.Localization;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddIdentityService(builder.Configuration);
builder.Services.AddScoped<UpdateUserUseCase>();
builder.Services.AddScoped<ResetUserPasswordUseCase>();
builder.Services.AddScoped<UnlockUserUseCase>();
builder.Services.AddScoped<ChangeOwnPasswordUseCase>();
builder.Services.AddScoped<GetRestaurantBranchesUseCase>();
builder.Services.AddScoped<CreateBranchUseCase>();
builder.Services.AddScoped<UpdateBranchUseCase>();
builder.Services.AddScoped<DeleteBranchUseCase>();
builder.Services.AddScoped<GetRestaurantUseCase>();
builder.Services.AddScoped<UpdateRestaurantUseCase>();
builder.Services.AddScoped<GetLoginAttemptsUseCase>();
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddPosAuthorizationPolicies();
builder.Services.AddPosAntiForgery();
builder.Services.AddControllers();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Middlewares
app.UseForwardedHeaders();
app.UsePosLocalization();
app.UseGlobalExceptionHandler();
app.UseCookieToBearer();
app.UseAuthentication();
app.UsePosCsrf();
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