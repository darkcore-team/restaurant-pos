using CatalogService.Infrastructure.Composition;
using CatalogService.Infrastructure.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using CatalogService.Api.Middleware;
using RestaurantPos.Shared.Infrastructure.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCatalogService(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

app.UsePosLocalization();
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions() { Predicate = check => check.Tags.Contains("live") });
app.MapControllers();


if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<CatalogDevSeeder>().SeedAsync();
}

app.Run();
