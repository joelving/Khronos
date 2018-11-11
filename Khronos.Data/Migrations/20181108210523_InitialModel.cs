using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Khronos.Data.Migrations
{
    public partial class InitialModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarFeeds",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarFeeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingSyncJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Owner = table.Column<string>(nullable: true),
                    FeedUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingSyncJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalendarSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CalendarId = table.Column<long>(nullable: false),
                    FetchedOn = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarSnapshots_CalendarFeeds_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "CalendarFeeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEvent",
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

            migrationBuilder.CreateIndex(
                name: "IX_CalendarSnapshots_CalendarId",
                table: "CalendarSnapshots",
                column: "CalendarId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarEvent");

            migrationBuilder.DropTable(
                name: "PendingSyncJobs");

            migrationBuilder.DropTable(
                name: "CalendarSnapshots");

            migrationBuilder.DropTable(
                name: "CalendarFeeds");
        }
    }
}
