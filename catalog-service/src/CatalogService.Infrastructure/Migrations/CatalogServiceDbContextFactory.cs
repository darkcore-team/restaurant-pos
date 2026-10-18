using CatalogService.Domain.Constants;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CatalogService.Infrastructure.Migrations;

public sealed class CatalogServiceDbContextFactory : IDesignTimeDbContextFactory<CatalogServiceDbContext>
{
    public CatalogServiceDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("CATALOG_DB_CONNECTION")
            ?? BuildConfiguration().GetConnectionString("CatalogDb")
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

    private static IConfiguration BuildConfiguration()
    {
        var basePath = Directory.GetCurrentDirectory();
        while (basePath is not null && !File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
            basePath = Directory.GetParent(basePath)?.FullName;
        }

        basePath ??= Directory.GetCurrentDirectory();

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}