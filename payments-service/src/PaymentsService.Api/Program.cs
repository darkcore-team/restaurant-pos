using PaymentsService.Infrastructure.Composition;
using PaymentsService.Infrastructure.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PaymentsService.Api.Middleware;
using RestaurantPos.Shared.Infrastructure.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPaymentsService(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UsePosLocalization();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions() { Predicate = check => check.Tags.Contains("live") });
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<PaymentsDevSeeder>().SeedAsync();
}

app.Run();