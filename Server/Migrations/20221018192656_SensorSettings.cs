using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ComplexPrototypeSystem.Server.Migrations
{
    public partial class SensorSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensorSettings",
                columns: table => new
                {
                    Guid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Interval = table.Column<int>(nullable: false),
                    IPAddressBytes = table.Column<byte[]>(maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorSettings", x => x.Guid);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorSettings");
        }
    }
}
