using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TablesService.Domain.Constants;
using TablesService.Infrastructure.Data;

namespace TablesService.Infrastructure.Migrations;

public sealed class TablesServiceDbContextFactory : IDesignTimeDbContextFactory<TablesServiceDbContext>
{
    public TablesServiceDbContext CreateDbContext(string[] args)
    {
        var configuration = Host.CreateDefaultBuilder(args)
            .Build()
            .Services
            .GetRequiredService<IConfiguration>();

        var connectionString = configuration.GetConnectionString("TablesDb")
            ?? Environment.GetEnvironmentVariable("TABLES_DB_CONNECTION")
            ?? "Host=localhost;Database=tables_design";

        var options = new DbContextOptionsBuilder<TablesServiceDbContext>()
            .UseNpgsql(connectionString, o =>
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "tables");
                o.MapEnum<RecordStatus>(enumName: "record_status", schemaName: "tables");
                o.MapEnum<TableStatus>(enumName: "table_status", schemaName: "tables");
            })
            .UseSnakeCaseNamingConvention()
            .Options;

        return new TablesServiceDbContext(options);
    }
}