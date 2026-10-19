using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "user_id1",
                schema: "identity",
                table: "user_branches",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_branches_user_id1",
                schema: "identity",
                table: "user_branches",
                column: "user_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_user_branches_users_user_id1",
                schema: "identity",
                table: "user_branches",
                column: "user_id1",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_branches_users_user_id1",
                schema: "identity",
                table: "user_branches");

            migrationBuilder.DropIndex(
                name: "ix_user_branches_user_id1",
                schema: "identity",
                table: "user_branches");

            migrationBuilder.DropColumn(
                name: "user_id1",
                schema: "identity",
                table: "user_branches");
        }
    }
}
