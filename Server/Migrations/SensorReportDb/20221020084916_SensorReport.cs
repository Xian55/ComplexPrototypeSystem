using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ComplexPrototypeSystem.Server.Migrations.SensorReportDb
{
    public partial class SensorReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensorReports",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(nullable: false),
                    SensorGuid = table.Column<Guid>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Usage = table.Column<int>(nullable: false),
                    TemperatureF = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReports", x => x.ReportId);
                });

            migrationBuilder.InsertData(
                table: "SensorReports",
                columns: new[] { "ReportId", "DateTime", "SensorGuid", "TemperatureF", "Usage" },
                values: new object[] { new Guid("cbf2d3ee-33d4-472e-a46b-7034329bcf45"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), 0, 0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorReports");
        }
    }
}
