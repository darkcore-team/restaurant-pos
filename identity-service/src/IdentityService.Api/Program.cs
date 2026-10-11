using IdentityService.Infrastructure.Composition;
using IdentityService.Infrastructure.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddIdentityService(builder.Configuration);
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Middlewares
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