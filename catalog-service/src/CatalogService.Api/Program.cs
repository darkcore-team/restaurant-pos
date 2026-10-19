using CatalogService.Infrastructure.Composition;
using CatalogService.Infrastructure.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCatalogService(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

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
