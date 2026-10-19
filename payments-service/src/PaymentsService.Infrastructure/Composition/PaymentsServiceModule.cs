using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantPos.Shared.Infrastructure.Authentication;
using RestaurantPos.Shared.Infrastructure.Health;
using RestaurantPos.Shared.Persistence;
using PaymentsService.Domain.Constants;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Infrastructure.Composition;

public static class PaymentsServiceModule
{
    public static IServiceCollection AddPaymentsService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentsDb")
            ?? throw new InvalidOperationException("Connection string 'PaymentsDb' not found");

        services.AddTenantHttpContext();
        services.AddTenantDbContext<PaymentsServiceDbContext>(
            connectionString, "payments",
            npgsql =>
            {
                npgsql.MapEnum<RecordStatus>(enumName: "record_status", schemaName: "payments");
                npgsql.MapEnum<PaymentStatus>(enumName: "payment_status", schemaName: "payments");
                npgsql.MapEnum<CashSessionStatus>(enumName: "cash_session_status", schemaName: "payments");
            },
            options => options.UseSnakeCaseNamingConvention());

        services.AddPosAuthentication(configuration);

        services.AddPosHealthChecks("payments-service");
        return services;
    }
}