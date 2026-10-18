using CatalogService.Infrastructure.Composition;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCatalogService(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions() { Predicate = check => check.Tags.Contains("live") });
app.MapControllers();

app.Run();
