using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace RestaurantPos.Shared.Infrastructure.Authentication;

public class CookieToBearerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            var accessToken = context.Request.Cookies[PosAuthCookies.AccessToken];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Request.Headers.Authorization = $"Bearer {accessToken}";
            }
        }

        await next(context);
    }
}

public static class CookieToBearerExtensions
{
    public static IApplicationBuilder UseCookieToBearer(this IApplicationBuilder app)
        => app.UseMiddleware<CookieToBearerMiddleware>();
}
