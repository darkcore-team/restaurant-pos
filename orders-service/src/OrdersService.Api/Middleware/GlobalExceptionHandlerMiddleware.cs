using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using RestaurantPos.Shared.Abstractions.Common;

namespace OrdersService.Api.Middleware;

/// <summary>
/// Middleware global de excepciones: loguea el detalle en el servidor
/// y devuelve un Answer generico al cliente (sin filtrar informacion interna).
/// </summary>
public static class GlobalExceptionHandlerMiddleware
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandler");

                var feature = context.Features.Get<IExceptionHandlerFeature>();
                if (feature?.Error is not null)
                    logger.LogError(feature.Error, "Unhandled exception on {Method} {Path}",
                        context.Request.Method, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var answer = Answer.Fail("INTERNAL_ERROR", "Error interno del servidor");
                await context.Response.WriteAsJsonAsync(answer);
            });
        });

        return app;
    }
}