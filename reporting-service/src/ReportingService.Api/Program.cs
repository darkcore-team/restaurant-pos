using ReportingService.Api.Middleware;
using ReportingService.Infrastructure.Composition;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using RestaurantPos.Shared.Infrastructure.Authentication;
using RestaurantPos.Shared.Infrastructure.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReportingService(builder.Configuration);
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
