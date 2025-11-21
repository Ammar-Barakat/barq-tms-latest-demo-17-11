using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarqTMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddClientIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "client_id",
                table: "USER",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_client_id",
                table: "USER",
                column: "client_id");

            migrationBuilder.AddForeignKey(
                name: "FK_USER_CLIENT_client_id",
                table: "USER",
                column: "client_id",
                principalTable: "CLIENT",
                principalColumn: "client_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USER_CLIENT_client_id",
                table: "USER");

            migrationBuilder.DropIndex(
                name: "IX_USER_client_id",
                table: "USER");

            migrationBuilder.DropColumn(
                name: "client_id",
                table: "USER");
        }
    }
}
