using Microsoft.EntityFrameworkCore.Migrations;

namespace RestaurantPos.Shared.Persistence.Migrations;

public static class RlsMigrationExtensions
{
    public static MigrationBuilder EnableRls(this MigrationBuilder mb, string schema, string table, string tenantColumn = "restaurant_id")
    {
        mb.Sql($"""
                ALTER TABLE {schema}.{table} ENABLE ROW LEVEL SECURITY;
                ALTER TABLE {schema}.{table} FORCE ROW LEVEL SECURITY;
                CREATE POLICY tenant_isolation ON {schema}.{table}
                    USING ({tenantColumn} = NULLIF(current_setting('app.restaurant_id', true), '')::bigint);
                """);
        return mb;
    }
}