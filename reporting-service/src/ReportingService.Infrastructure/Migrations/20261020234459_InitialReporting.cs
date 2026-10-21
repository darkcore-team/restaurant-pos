using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReportingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reporting");
            
            migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS reporting;");

            migrationBuilder.CreateTable(
                name: "daily_product_sales",
                schema: "reporting",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    business_date = table.Column<DateOnly>(type: "date", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    total = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_product_sales", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "daily_sales",
                schema: "reporting",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    business_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_sales = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    total_discount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    total_tax = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    total_tips = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false),
                    by_payment_method = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_sales", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "daily_waiter_sales",
                schema: "reporting",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    restaurant_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    business_date = table.Column<DateOnly>(type: "date", nullable: false),
                    waiter_id = table.Column<long>(type: "bigint", nullable: false),
                    total = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    tips = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_waiter_sales", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_daily_product_sales_restaurant_id_branch_id_business_date_p",
                schema: "reporting",
                table: "daily_product_sales",
                columns: new[] { "restaurant_id", "branch_id", "business_date", "product_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_daily_sales_restaurant_id_branch_id_business_date",
                schema: "reporting",
                table: "daily_sales",
                columns: new[] { "restaurant_id", "branch_id", "business_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_daily_waiter_sales_restaurant_id_branch_id_business_date_wa",
                schema: "reporting",
                table: "daily_waiter_sales",
                columns: new[] { "restaurant_id", "branch_id", "business_date", "waiter_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_product_sales",
                schema: "reporting");

            migrationBuilder.DropTable(
                name: "daily_sales",
                schema: "reporting");

            migrationBuilder.DropTable(
                name: "daily_waiter_sales",
                schema: "reporting");
        }
    }
}
