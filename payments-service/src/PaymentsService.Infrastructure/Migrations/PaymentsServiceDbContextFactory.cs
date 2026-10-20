using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentsService.Domain.Constants;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Infrastructure.Migrations;

public sealed class PaymentsServiceDbContextFactory : IDesignTimeDbContextFactory<PaymentsServiceDbContext>
{
    public PaymentsServiceDbContext CreateDbContext(string[] args)
    {
        var configuration = Host.CreateDefaultBuilder(args)
            .Build()
            .Services
            .GetRequiredService<IConfiguration>();

        var connectionString = configuration.GetConnectionString("PaymentsDb")
            ?? Environment.GetEnvironmentVariable("PAYMENTS_DB_CONNECTION")
            ?? "Host=localhost;Database=payments_design";

        var options = new DbContextOptionsBuilder<PaymentsServiceDbContext>()
            .UseNpgsql(connectionString, o =>
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "payments");
                o.MapEnum<RecordStatus>(enumName: "record_status", schemaName: "payments");
                o.MapEnum<PaymentStatus>(enumName: "payment_status", schemaName: "payments");
                o.MapEnum<CashSessionStatus>(enumName: "cash_session_status", schemaName: "payments");
            })
            .UseSnakeCaseNamingConvention()
            .Options;

        return new PaymentsServiceDbContext(options);
    }
}