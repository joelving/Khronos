using Microsoft.EntityFrameworkCore.Migrations;

namespace Khronos.Data.Migrations
{
    public partial class CalendarShortname : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CalendarFeeds",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "CalendarFeeds");
        }
    }
}
