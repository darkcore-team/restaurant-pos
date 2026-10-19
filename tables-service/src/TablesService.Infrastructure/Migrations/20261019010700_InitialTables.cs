using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TablesService.Domain.Constants;

#nullable disable

namespace TablesService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tables");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:tables.record_status", "ACTIVE,DELETED,INACTIVE")
                .Annotation("Npgsql:Enum:tables.table_status", "BILLING,DIRTY,FREE,OCCUPIED,OUT_OF_SERVICE,RESERVED");

            migrationBuilder.CreateTable(
                name: "zones",
                schema: "tables",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    position = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_zones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dining_tables",
                schema: "tables",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    zone_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    min_capacity = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    max_capacity = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)4),
                    is_combinable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    status = table.Column<TableStatus>(type: "tables.table_status", nullable: false, defaultValue: TableStatus.Free),
                    position = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    notes = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dining_tables", x => x.id);
                    table.ForeignKey(
                        name: "fk_dining_tables_zones_zone_id",
                        column: x => x.zone_id,
                        principalSchema: "tables",
                        principalTable: "zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "table_assignments",
                schema: "tables",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    table_id = table.Column<long>(type: "bigint", nullable: false),
                    waiter_id = table.Column<long>(type: "bigint", nullable: false),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    assigned_by = table.Column<long>(type: "bigint", nullable: true),
                    released_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    released_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_table_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_table_assignments_dining_tables_table_id",
                        column: x => x.table_id,
                        principalSchema: "tables",
                        principalTable: "dining_tables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dining_tables_zone_id",
                schema: "tables",
                table: "dining_tables",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_table_branch",
                schema: "tables",
                table: "dining_tables",
                columns: new[] { "branch_id", "zone_id", "position" });

            migrationBuilder.CreateIndex(
                name: "uq_table_code",
                schema: "tables",
                table: "dining_tables",
                columns: new[] { "branch_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_table_assignments_table_id",
                schema: "tables",
                table: "table_assignments",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "ix_zone_branch_pos",
                schema: "tables",
                table: "zones",
                columns: new[] { "branch_id", "restaurant_id", "position" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "table_assignments",
                schema: "tables");

            migrationBuilder.DropTable(
                name: "dining_tables",
                schema: "tables");

            migrationBuilder.DropTable(
                name: "zones",
                schema: "tables");
        }
    }
}
