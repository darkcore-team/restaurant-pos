using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using RestaurantPos.Shared.Abstractions.Common;

namespace ReportingService.Api.Middleware;

public static class GlobalExceptionHandlerMiddleware
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                if (feature?.Error is not null) logger.LogError(feature.Error, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(Answer.Fail("INTERNAL_ERROR", "Error interno del servidor"));
            });
        });
        return app;
    }
}
