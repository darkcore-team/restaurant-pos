using CatalogService.Domain.Constants;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatalogService.Infrastructure.Migrations;

public sealed class CatalogServiceDbContextFactory : IDesignTimeDbContextFactory<CatalogServiceDbContext>
{
    public CatalogServiceDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("CATALOG_DB_CONNECTION")
            ?? "Host=localhost;Database=catalog_design";

        var options = new DbContextOptionsBuilder<CatalogServiceDbContext>()
            .UseNpgsql(connectionString, o =>
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "catalog");
                o.MapEnum<RecordStatus>(enumName: "record_status", schemaName: "catalog");
            })
            .UseSnakeCaseNamingConvention()
            .Options;

        return new CatalogServiceDbContext(options);
    }
}