using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantPos.Shared.Infrastructure.Localization;

public static class LocalizationExtensions
{
    private static readonly string[] Supported = { "es", "en" };

    public static IServiceCollection AddPosLocalization(this IServiceCollection services)
    {
        services.AddLocalization();
        return services;
    }

    public static IApplicationBuilder UsePosLocalization(this IApplicationBuilder app)
    {
        var options = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("es"),
            SupportedCultures = Supported.Select(c => new CultureInfo(c)).ToList(),
            SupportedUICultures = Supported.Select(c => new CultureInfo(c)).ToList()
        };
        // Accept-Language header (standar) has priority
        options.RequestCultureProviders = new List<IRequestCultureProvider>
        {
            new AcceptLanguageHeaderRequestCultureProvider()
        };
        return app.UseRequestLocalization(options);
    }
}