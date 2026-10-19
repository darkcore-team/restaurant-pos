using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace RestaurantPos.Shared.Infrastructure.Authentication;

public static class PosAuthenticationExtensions
{
    public static IServiceCollection AddPosAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwt = configuration.GetSection(PosJwtOptions.SectionName).Get<PosJwtOptions>()
                  ?? throw new InvalidOperationException(
                      $"Missing configuration section '{PosJwtOptions.SectionName}'.");

        if (string.IsNullOrWhiteSpace(jwt.Authority) || string.IsNullOrWhiteSpace(jwt.Audience))
            throw new InvalidOperationException("Jwt:Authority and Jwt:Audience are both required. " +
                                                "Authority points ar identity-service; Audience is the resource server identifier.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwt.Authority;
                options.Audience = jwt.Audience;

                options.RequireHttpsMetadata = jwt.RequireHttpsMetadata;
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "name",
                    RoleClaimType = "role",
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddPosAuthorizationPolicies();
        return services;
    }

    public static IServiceCollection AddPosAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(PosPolicies.TenantAccess, p => p.RequireClaim(PosClaims.RestaurantId))
            .AddPolicy(PosPolicies.WaiterOrAbove, p => p.RequireRole("ADMIN", "OWNER", "MANAGER", "WAITER", "HOST"))
            .AddPolicy(PosPolicies.ManagerOrAbove, p => p.RequireRole("ADMIN", "OWNER", "MANAGER"))
            .AddPolicy(PosPolicies.CashierOrAbove, p => p.RequireRole("ADMIN", "OWNER", "MANAGER", "CASHIER"))
            .AddPolicy(PosPolicies.AdminOnly, p => p.RequireRole("ADMIN", "OWNER"));
        return services;
    }
}
