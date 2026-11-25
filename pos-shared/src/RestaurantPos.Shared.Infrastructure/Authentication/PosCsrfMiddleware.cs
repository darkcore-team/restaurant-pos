using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RestaurantPos.Shared.Abstractions.Common;

namespace RestaurantPos.Shared.Infrastructure.Authentication;

public class PosCsrfMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Get, HttpMethods.Head, HttpMethods.Options, HttpMethods.Trace
    };

    public async Task InvokeAsync(HttpContext context)
    {
        if (SafeMethods.Contains(context.Request.Method) || IsExempt(context.Request.Path))
        {
            await next(context);
            return;
        }

        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();

        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            context.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(Answer.Fail("CSRF_TOKEN_INVALID", "Token CSRF ausente o invalido."));
            return;
        }

        await next(context);
    }

    private static bool IsExempt(PathString path)
    {
        return path.StartsWithSegments("/hubs") || path.StartsWithSegments("/connect");
    }
}

public static class PosCsrfExtensions
{
    public static IApplicationBuilder UsePosCsrf(this IApplicationBuilder app)
        => app.UseMiddleware<PosCsrfMiddleware>();
}
