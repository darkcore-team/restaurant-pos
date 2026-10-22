using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantPos.Shared.Infrastructure.Authentication;

public static class PosAntiForgeryExtensions
{
    public const string HeaderName = PosAuthCookies.CsrfHeader;

    public static IServiceCollection AddPosAntiForgery(this IServiceCollection services)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = HeaderName;
            options.Cookie.Name = PosAuthCookies.Csrf;
            options.Cookie.Path = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        return services;
    }

    public class Filter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
            await antiforgery.ValidateRequestAsync(context.HttpContext);
            return await next(context);
        }
    }
}
