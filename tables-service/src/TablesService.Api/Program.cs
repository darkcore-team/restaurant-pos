using TablesService.Infrastructure.Composition;
using TablesService.Infrastructure.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using RestaurantPos.Shared.Infrastructure.Localization;
using TablesService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTablesService(builder.Configuration);
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
    await scope.ServiceProvider.GetRequiredService<TablesDevSeeder>().SeedAsync();
}

app.Run();