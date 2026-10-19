using System;
using CatalogService.Domain.Constants;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RestaurantPos.Shared.Persistence.Migrations;

#nullable disable

namespace CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:catalog.record_status", "ACTIVE,DELETED,INACTIVE");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    name_en = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    position = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0"),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "currencies",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    symbol = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    decimal_places = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)2),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_currencies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "languages",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_languages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "option_groups",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    min_select = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    max_select = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    position = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0"),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_option_groups", x => x.id);
                    table.CheckConstraint("ck_og_select", "\"min_select\" >= 0 AND \"max_select\" >= 1 AND \"min_select\" <= \"max_select\"");
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    requires_reference = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    requires_amount_tendered = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    requires_cash_session = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    position = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0"),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_methods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stations",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    color = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    position = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0"),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    code_alpha3 = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    default_currency_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.id);
                    table.ForeignKey(
                        name: "fk_countries_currencies_default_currency_id",
                        column: x => x.default_currency_id,
                        principalSchema: "catalog",
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_options",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    option_group_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    name_en = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    price_delta = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    position = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0"),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_options", x => x.id);
                    table.CheckConstraint("ck_po_delta", "\"price_delta\" >= 0");
                    table.ForeignKey(
                        name: "fk_product_options_option_groups_option_group_id",
                        column: x => x.option_group_id,
                        principalSchema: "catalog",
                        principalTable: "option_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_types",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    country_id = table.Column<long>(type: "bigint", nullable: true),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    requires_expiry_date = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_document_types_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "catalog",
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "regions",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    country_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regions", x => x.id);
                    table.ForeignKey(
                        name: "fk_regions_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "catalog",
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "taxes",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    country_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    rate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    is_inclusive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_taxes", x => x.id);
                    table.CheckConstraint("ck_tax_rate", "\"rate\" >= 0 AND \"rate\" <= 1");
                    table.ForeignKey(
                        name: "fk_taxes_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "catalog",
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cities",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    country_id = table.Column<long>(type: "bigint", nullable: false),
                    region_id = table.Column<long>(type: "bigint", nullable: true),
                    code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    is_capital = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cities", x => x.id);
                    table.ForeignKey(
                        name: "fk_cities_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "catalog",
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cities_regions_region_id",
                        column: x => x.region_id,
                        principalSchema: "catalog",
                        principalTable: "regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    station_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    name_en = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    description_en = table.Column<string>(type: "text", nullable: true),
                    base_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    is_available = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    position = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0"),
                    tax_id = table.Column<long>(type: "bigint", nullable: true),
                    is_tax_exempt = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.CheckConstraint("ck_product_price", "\"base_price\" >= 0");
                    table.ForeignKey(
                        name: "fk_products_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_products_stations_station_id",
                        column: x => x.station_id,
                        principalSchema: "catalog",
                        principalTable: "stations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_taxes_tax_id",
                        column: x => x.tax_id,
                        principalSchema: "catalog",
                        principalTable: "taxes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "product_option_groups",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    option_group_id = table.Column<long>(type: "bigint", nullable: false),
                    min_select = table.Column<short>(type: "smallint", nullable: true),
                    max_select = table.Column<short>(type: "smallint", nullable: true),
                    position = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0"),
                    status = table.Column<RecordStatus>(type: "catalog.record_status", nullable: false, defaultValueSql: "'ACTIVE'"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_option_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_option_groups_option_groups_option_group_id",
                        column: x => x.option_group_id,
                        principalSchema: "catalog",
                        principalTable: "option_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_option_groups_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categories_branch_id_restaurant_id_position",
                schema: "catalog",
                table: "categories",
                columns: new[] { "branch_id", "restaurant_id", "position" });

            migrationBuilder.CreateIndex(
                name: "ix_categories_id_restaurant_id",
                schema: "catalog",
                table: "categories",
                columns: new[] { "id", "restaurant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_public_id",
                schema: "catalog",
                table: "categories",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cities_country_id_name",
                schema: "catalog",
                table: "cities",
                columns: new[] { "country_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cities_public_id",
                schema: "catalog",
                table: "cities",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cities_region_id",
                schema: "catalog",
                table: "cities",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "ix_countries_code",
                schema: "catalog",
                table: "countries",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_countries_code_alpha3",
                schema: "catalog",
                table: "countries",
                column: "code_alpha3",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_countries_default_currency_id",
                schema: "catalog",
                table: "countries",
                column: "default_currency_id");

            migrationBuilder.CreateIndex(
                name: "ix_countries_public_id",
                schema: "catalog",
                table: "countries",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_currencies_code",
                schema: "catalog",
                table: "currencies",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_currencies_public_id",
                schema: "catalog",
                table: "currencies",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_document_types_country_id_code",
                schema: "catalog",
                table: "document_types",
                columns: new[] { "country_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_document_types_public_id",
                schema: "catalog",
                table: "document_types",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_languages_code",
                schema: "catalog",
                table: "languages",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_languages_public_id",
                schema: "catalog",
                table: "languages",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_option_groups_id_restaurant_id",
                schema: "catalog",
                table: "option_groups",
                columns: new[] { "id", "restaurant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_option_groups_public_id",
                schema: "catalog",
                table: "option_groups",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_methods_code",
                schema: "catalog",
                table: "payment_methods",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_methods_public_id",
                schema: "catalog",
                table: "payment_methods",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_option_groups_option_group_id",
                schema: "catalog",
                table: "product_option_groups",
                column: "option_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_option_groups_product_id_option_group_id",
                schema: "catalog",
                table: "product_option_groups",
                columns: new[] { "product_id", "option_group_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_option_groups_public_id",
                schema: "catalog",
                table: "product_option_groups",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_options_id_restaurant_id",
                schema: "catalog",
                table: "product_options",
                columns: new[] { "id", "restaurant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_options_option_group_id_position",
                schema: "catalog",
                table: "product_options",
                columns: new[] { "option_group_id", "position" });

            migrationBuilder.CreateIndex(
                name: "ix_product_options_public_id",
                schema: "catalog",
                table: "product_options",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_branch_id_restaurant_id_status_category_id_position",
                schema: "catalog",
                table: "products",
                columns: new[] { "branch_id", "restaurant_id", "status", "category_id", "position" });

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                schema: "catalog",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_id_restaurant_id",
                schema: "catalog",
                table: "products",
                columns: new[] { "id", "restaurant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_public_id",
                schema: "catalog",
                table: "products",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_station_id",
                schema: "catalog",
                table: "products",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_tax_id",
                schema: "catalog",
                table: "products",
                column: "tax_id");

            migrationBuilder.CreateIndex(
                name: "ix_regions_country_id_code",
                schema: "catalog",
                table: "regions",
                columns: new[] { "country_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_regions_public_id",
                schema: "catalog",
                table: "regions",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stations_id_restaurant_id",
                schema: "catalog",
                table: "stations",
                columns: new[] { "id", "restaurant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stations_public_id",
                schema: "catalog",
                table: "stations",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stations_restaurant_id_code",
                schema: "catalog",
                table: "stations",
                columns: new[] { "restaurant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_taxes_country_id_code",
                schema: "catalog",
                table: "taxes",
                columns: new[] { "country_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_taxes_public_id",
                schema: "catalog",
                table: "taxes",
                column: "public_id",
                unique: true);
            // RLS for 6 business tables
            migrationBuilder.EnableRls("catalog", "stations");
            migrationBuilder.EnableRls("catalog", "categories");
            migrationBuilder.EnableRls("catalog", "products");
            migrationBuilder.EnableRls("catalog", "option_groups");
            migrationBuilder.EnableRls("catalog", "product_options");
            migrationBuilder.EnableRls("catalog", "product_option_groups");
            
            // perms for pos_app rol
            migrationBuilder.Sql("GRANT USAGE ON SCHEMA catalog TO pos_app;");                                      
            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA catalog TO pos_app;");
            migrationBuilder.Sql("GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA catalog TO pos_app;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON catalog.stations;");                    
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON catalog.categories;");                  
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON catalog.products;");                    
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON catalog.option_groups;");               
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON catalog.product_options;");             
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON catalog.product_option_groups;");
            migrationBuilder.DropTable(
                name: "cities",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "document_types",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "languages",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "payment_methods",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_option_groups",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_options",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "regions",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "products",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "option_groups",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "stations",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "taxes",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "countries",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "currencies",
                schema: "catalog");
        }
    }
}
