using ReportingService.Api.Middleware;
using ReportingService.Infrastructure.Composition;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using RestaurantPos.Shared.Infrastructure.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReportingService(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

app.UseGlobalExceptionHandler();

app.UsePosLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions() { Predicate = check => check.Tags.Contains("live") });

app.MapControllers();

app.Run();
