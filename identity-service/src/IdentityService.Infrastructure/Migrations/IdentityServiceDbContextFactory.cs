using IdentityService.Domain.Constants;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityService.Infrastructure.Migrations;

public sealed class IdentityServiceDbContextFactory : IDesignTimeDbContextFactory<IdentityServiceDbContext>
{
    public IdentityServiceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=identity_design", o =>
            {
                o.MigrationsHistoryTable("__EFMigrationsHistory", "identity");
                o.MapEnum<RecordStatus>(enumName: "record_status", schemaName: "identity");
            })
            .UseSnakeCaseNamingConvention()
            .Options;

        return new IdentityServiceDbContext(options);
    }
}