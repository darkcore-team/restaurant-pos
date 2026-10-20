using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using PaymentsService.Application.CashRegisters;
using PaymentsService.Application.CashSessions;
using PaymentsService.Application.Payments;
using RestaurantPos.Shared.Infrastructure.Authentication;
using RestaurantPos.Shared.Infrastructure.Health;
using RestaurantPos.Shared.Persistence;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Repositories;
using PaymentsService.Infrastructure.Data;
using PaymentsService.Infrastructure.Repositories;
using PaymentsService.Infrastructure.Seed;
using RestaurantPos.Shared.Infrastructure.Localization;
using RestaurantPos.Shared.Infrastructure.Messaging;
using RestaurantPos.Shared.Persistence.Outbox;

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
        services.AddPosLocalization();
        
        services.AddTransient<IStringLocalizer>(sp =>
        {
            var factory = sp.GetRequiredService<IStringLocalizerFactory>();
            var assemblyName = "PaymentsService.Application";
            return factory.Create($"{assemblyName}.Resources.Messages", assemblyName);
        });
        
        services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
        services.AddScoped<ICashSessionRepository, CashSessionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        
        services.AddScoped<GetCashRegistersUseCase>();
        services.AddScoped<OpenCashSessionUseCase>();
        services.AddScoped<CloseCashSessionUseCase>();
        services.AddScoped<GetActiveCashSessionUseCase>();
        services.AddScoped<RegisterPaymentUseCase>();
        services.AddScoped<CancelPaymentUseCase>();
        services.AddScoped<GetOrderPaymentsUseCase>();
        services.AddScoped<GetTicketUseCase>();
        services.AddScoped<PaymentsDevSeeder>();

        services.AddPosHealthChecks("payments-service");

        services.AddPosMessaging(configuration);
        services.AddOutboxPublisher<PaymentsServiceDbContext>(configuration);
        
        return services;
    }
}