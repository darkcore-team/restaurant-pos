using IdentityService.Domain.Constants;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Authentication;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Server;
using RestaurantPos.Shared.Infrastructure.Health;
using RestaurantPos.Shared.Infrastructure.Localization;
using RestaurantPos.Shared.Persistence;
using IdentityOptions = IdentityService.Infrastructure.Configuration.IdentityOptions;

namespace IdentityService.Infrastructure.Composition;

public static class IdentityServiceModule
{
    public static IServiceCollection AddIdentityService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityDb")
            ?? throw new InvalidOperationException("Connection string 'IdentityDb' not found");

        var identity = configuration.GetSection(IdentityOptions.SectionName);
        services.Configure<IdentityOptions>(identity);

        services.AddOptions<IdentityOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTenantHttpContext();
        services.AddTenantDbContext<IdentityServiceDbContext>(
            connectionString, "identity",
            npgsql => npgsql.MapEnum<RecordStatus>(enumName: "record_status", schemaName: "identity"),
            options => options.UseSnakeCaseNamingConvention());

        var identityOptions = identity.Get<IdentityOptions>() ?? new IdentityOptions();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<OpenIddictPasswordGrantHandler>();
        services.AddScoped<IdentityDevSeeder>();
        services.AddScoped<IUserBranchRepository, UserBranchRepository>();
        services.AddScoped<IAuthorityRepository, AuthorityRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddOpenIddict()
            .AddCore(options => options
                .UseEntityFrameworkCore()
                .UseDbContext<IdentityServiceDbContext>())
            .AddServer(options =>
            {
                options.SetIssuer(identity["Issuer"] ?? "https://localhost:8081");
                options.SetTokenEndpointUris("/connect/token");
                
                options.AllowPasswordFlow();
                options.AllowRefreshTokenFlow();
                options.AllowClientCredentialsFlow();
                
                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(identityOptions.AccessTokenExpiration));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(14));
                
                options.AddEphemeralSigningKey();
                options.AddEphemeralEncryptionKey();
                options.DisableAccessTokenEncryption();
                
                options.AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>(handler =>
                    handler.UseScopedHandler<OpenIddictPasswordGrantHandler>());

                options.UseAspNetCore().DisableTransportSecurityRequirement();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();

                options.Configure(valOptions =>
                {
                    valOptions.TokenValidationParameters.NameClaimType = "name";
                    valOptions.TokenValidationParameters.RoleClaimType = "role";
                });
            });

        services.AddPosLocalization();
        
        services.AddTransient<Microsoft.Extensions.Localization.IStringLocalizer>(sp =>
        {
            var factory = sp.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizerFactory>();
            var assemblyName = "IdentityService.Application";
            return factory.Create($"{assemblyName}.Resources.Messages", assemblyName);
        });

        services.AddPosHealthChecks("identity-service");
        return services;
    }
}