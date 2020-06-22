using System;
using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;
using EDennis.MigrationsExtensions;

namespace Hr.PersonApi.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateMaintenanceProcedures();
            migrationBuilder.CreateTestJsonTableSupport();
            
            migrationBuilder.CreateSequence<int>(
                name: "seqAddress");

            migrationBuilder.CreateSequence<int>(
                name: "seqPerson");

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValueSql: "next value for seqPerson"),
                    SysUser = table.Column<string>(nullable: true),
                    SysStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    SysStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SysEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstName = table.Column<string>(maxLength: 40, nullable: true),
                    LastName = table.Column<string>(maxLength: 40, nullable: true),
                    DateOfBirth = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "State",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    SysUser = table.Column<string>(nullable: true),
                    SysStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    SysStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SysEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_State", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValueSql: "next value for seqAddress"),
                    SysUser = table.Column<string>(nullable: true),
                    SysStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    SysStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SysEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StreetAddress = table.Column<string>(maxLength: 100, nullable: true),
                    City = table.Column<string>(maxLength: 40, nullable: true),
                    State = table.Column<string>(maxLength: 2, nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    PersonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Address_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_PersonId",
                table: "Address",
                column: "PersonId");

            migrationBuilder.SaveMappings();
            migrationBuilder.CreateSqlServerTemporalTables();
            //migrationBuilder.Sql(File.ReadAllText("MigrationsInserts\\Initial.sql"));

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "State");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropSequence(
                name: "seqAddress");

            migrationBuilder.DropSequence(
                name: "seqPerson");
        }
    }
}
