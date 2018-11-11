using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Khronos.Data.Migrations
{
    public partial class EventAccessor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarEvent");

            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SnapshotId = table.Column<long>(nullable: false),
                    UId = table.Column<string>(nullable: true),
                    Start = table.Column<long>(nullable: false),
                    End = table.Column<long>(nullable: false),
                    Duration = table.Column<double>(nullable: false),
                    Status = table.Column<int>(nullable: true),
                    Summary = table.Column<string>(nullable: true),
                    _attendees = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_CalendarSnapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "CalendarSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_SnapshotId",
                table: "CalendarEvents",
                column: "SnapshotId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarEvents");

            migrationBuilder.CreateTable(
                name: "CalendarEvent",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Duration = table.Column<double>(nullable: false),
                    End = table.Column<long>(nullable: false),
                    SnapshotId = table.Column<long>(nullable: false),
                    Start = table.Column<long>(nullable: false),
                    Status = table.Column<int>(nullable: true),
                    Summary = table.Column<string>(nullable: true),
                    UId = table.Column<string>(nullable: true),
                    _attendees = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarEvent_CalendarSnapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "CalendarSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvent_SnapshotId",
                table: "CalendarEvent",
                column: "SnapshotId");
        }
    }
}
