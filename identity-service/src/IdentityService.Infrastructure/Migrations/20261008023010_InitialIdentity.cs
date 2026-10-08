using System;
using System.Text.Json;
using IdentityService.Domain.Constants;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RestaurantPos.Shared.Persistence;
using RestaurantPos.Shared.Persistence.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:identity.record_status", "ACTIVE,DELETED,INACTIVE");

            migrationBuilder.CreateTable(
                name: "authorities",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    position = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0"),
                    status = table.Column<RecordStatus>(type: "identity.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authorities", x => x.id);
                    table.CheckConstraint("ck_auth_code", "\"code\" ~ '^[A-Z][A-Z0-9_]*$'");
                });

            migrationBuilder.CreateTable(
                name: "people",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    first_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    last_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    photo_url = table.Column<string>(type: "text", nullable: true),
                    document_type_id = table.Column<long>(type: "bigint", nullable: true),
                    document_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    city_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<RecordStatus>(type: "identity.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_people", x => x.id);
                    table.CheckConstraint("ck_person_doc", "(\"document_type_id\" IS NULL) = (\"document_number\" IS NULL)");
                });

            migrationBuilder.CreateTable(
                name: "restaurants",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tax_id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "'America/Managua'"),
                    default_language_id = table.Column<long>(type: "bigint", nullable: false),
                    default_currency_id = table.Column<long>(type: "bigint", nullable: false),
                    default_country_id = table.Column<long>(type: "bigint", nullable: true),
                    base_currency_id = table.Column<long>(type: "bigint", nullable: false),
                    settings = table.Column<JsonElement>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    status = table.Column<RecordStatus>(type: "identity.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "people_contacts",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    person_id = table.Column<long>(type: "bigint", nullable: false),
                    contact_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    contact_value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    status = table.Column<RecordStatus>(type: "identity.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_people_contacts", x => x.id);
                    table.CheckConstraint("ck_contact_type", "\"contact_type\" IN ('EMAIL','PHONE','ADDRESS')");
                    table.ForeignKey(
                        name: "fk_people_contacts_people_person_id",
                        column: x => x.person_id,
                        principalSchema: "identity",
                        principalTable: "people",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    person_id = table.Column<long>(type: "bigint", nullable: false),
                    username = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    password_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    failed_count = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0"),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<RecordStatus>(type: "identity.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_people_person_id",
                        column: x => x.person_id,
                        principalSchema: "identity",
                        principalTable: "people",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "branches",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    city_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    opens_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    closes_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    status = table.Column<RecordStatus>(type: "identity.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_branches", x => x.id);
                    table.CheckConstraint("ck_branch_hours", "\"closes_at\" IS NULL OR \"opens_at\" IS NULL OR \"closes_at\" > \"opens_at\"");
                    table.ForeignKey(
                        name: "fk_branches_restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalSchema: "identity",
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_authorities",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    authority_id = table.Column<long>(type: "bigint", nullable: false),
                    granted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    granted_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_authorities", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_authorities_authorities_authority_id",
                        column: x => x.authority_id,
                        principalSchema: "identity",
                        principalTable: "authorities",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_authorities_users_granted_by",
                        column: x => x.granted_by,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_authorities_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_branches",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_branches", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_branches_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "identity",
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_branches_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_authorities_code",
                schema: "identity",
                table: "authorities",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_authorities_public_id",
                schema: "identity",
                table: "authorities",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_branches_id_restaurant_id",
                schema: "identity",
                table: "branches",
                columns: new[] { "id", "restaurant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_branches_public_id",
                schema: "identity",
                table: "branches",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_branches_restaurant_id",
                schema: "identity",
                table: "branches",
                column: "restaurant_id",
                unique: true,
                filter: "\"is_default\"");

            migrationBuilder.CreateIndex(
                name: "ix_branches_restaurant_id_code",
                schema: "identity",
                table: "branches",
                columns: new[] { "restaurant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_people_name",
                schema: "identity",
                table: "people",
                columns: new[] { "last_name", "first_name" });

            migrationBuilder.CreateIndex(
                name: "ix_people_public_id",
                schema: "identity",
                table: "people",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_people_document",
                schema: "identity",
                table: "people",
                columns: new[] { "document_type_id", "document_number" },
                unique: true,
                filter: "\"document_number\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_people_contacts_public_id",
                schema: "identity",
                table: "people_contacts",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_contact_value",
                schema: "identity",
                table: "people_contacts",
                columns: new[] { "person_id", "contact_type", "contact_value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_restaurants_public_id",
                schema: "identity",
                table: "restaurants",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_restaurants_tax_id",
                schema: "identity",
                table: "restaurants",
                column: "tax_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_authorities_authority_id",
                schema: "identity",
                table: "user_authorities",
                column: "authority_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_authorities_granted_by",
                schema: "identity",
                table: "user_authorities",
                column: "granted_by");

            migrationBuilder.CreateIndex(
                name: "uq_user_authority",
                schema: "identity",
                table: "user_authorities",
                columns: new[] { "user_id", "authority_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_branches_branch_id",
                schema: "identity",
                table: "user_branches",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "uq_user_branch",
                schema: "identity",
                table: "user_branches",
                columns: new[] { "user_id", "branch_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_person_id",
                schema: "identity",
                table: "users",
                column: "person_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_public_id",
                schema: "identity",
                table: "users",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                schema: "identity",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.EnableRls("identity", "restaurants", "id");
            migrationBuilder.EnableRls("identity", "branches");
            migrationBuilder.Sql("GRANT USAGE ON SCHEMA identity to pos_app");
            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA identity TO pos_app;");
            migrationBuilder.Sql("GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA identity TO pos_app;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON identity.restaurants;");
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON identity.branches;");
            
            migrationBuilder.DropTable(
                name: "people_contacts",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_authorities",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_branches",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "authorities",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "branches",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "restaurants",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "people",
                schema: "identity");
        }
    }
}
