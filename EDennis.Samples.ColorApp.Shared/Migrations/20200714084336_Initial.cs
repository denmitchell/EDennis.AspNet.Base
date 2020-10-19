using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDennis.Samples.ColorApp.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "seqRgb");

            migrationBuilder.CreateTable(
                name: "Rgb",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValueSql: "next value for seqRgb"),
                    SysUser = table.Column<string>(nullable: true),
                    SysStatus = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    SysStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SysEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Red = table.Column<int>(nullable: false),
                    Green = table.Column<int>(nullable: false),
                    Blue = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rgb", x => x.Id);
                });
            migrationBuilder.Sql(File.ReadAllText("MigrationsInserts\\Initial.sql"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rgb");

            migrationBuilder.DropSequence(
                name: "seqRgb");
        }
    }
}
