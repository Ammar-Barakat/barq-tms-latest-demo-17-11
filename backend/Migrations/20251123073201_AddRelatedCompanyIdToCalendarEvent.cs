using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarqTMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatedCompanyIdToCalendarEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelatedCompanyId",
                table: "CalendarEvents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_RelatedCompanyId",
                table: "CalendarEvents",
                column: "RelatedCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Companies_RelatedCompanyId",
                table: "CalendarEvents",
                column: "RelatedCompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_Companies_RelatedCompanyId",
                table: "CalendarEvents");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_RelatedCompanyId",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "RelatedCompanyId",
                table: "CalendarEvents");
        }
    }
}
