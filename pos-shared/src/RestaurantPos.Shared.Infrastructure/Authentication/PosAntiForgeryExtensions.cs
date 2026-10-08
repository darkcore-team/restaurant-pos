using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantPos.Shared.Infrastructure.Authentication;

public static class PosAntiForgeryExtensions
{
    public const string HeaderName = "X-CSRF-TOKEN";

    public static IServiceCollection AddPosAntiForgery(this IServiceCollection services)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = HeaderName;
            options.Cookie.Name = "pos_csrf_token";
            // The SPA has to read this cookie to echo it back in the header, so it cannot be HttpOnly
            options.Cookie.HttpOnly = false;
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