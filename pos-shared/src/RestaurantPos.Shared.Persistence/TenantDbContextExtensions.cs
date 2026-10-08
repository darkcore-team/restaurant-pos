using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using RestaurantPos.Shared.Abstractions;
using RestaurantPos.Shared.Abstractions.Tenancy;
using RestaurantPos.Shared.Persistence.Interceptors;

namespace RestaurantPos.Shared.Persistence;

public static class TenantDbContextExtensions
{
    public static IServiceCollection AddTenantDbContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        string schema,
        Action<NpgsqlDbContextOptionsBuilder>? npgsql = null,
        Action<DbContextOptionsBuilder>? options = null)
        where TContext : DbContext
    {
        services.AddScoped<RlsConnectionInterceptor>();
        services.AddDbContext<TContext>((sp, builder) =>
        {
            builder.UseNpgsql(connectionString, o =>
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", schema);
                npgsql?.Invoke(o);
            });
            options?.Invoke(builder);
            builder.AddInterceptors(sp.GetRequiredService<RlsConnectionInterceptor>());
        });
        
        return services;
    }

    public static IServiceCollection AddTenantHttpContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, ScopedTenantContext>();
        return services;
    }

    public static IServiceCollection AddTenantBackgroundContext(this IServiceCollection services)
    {
        services.AddSingleton<BackgroundTenantContext>();
        services.AddSingleton<ITenantContextSetter>(sp => sp.GetRequiredService<BackgroundTenantContext>());
        return services;
    }
}