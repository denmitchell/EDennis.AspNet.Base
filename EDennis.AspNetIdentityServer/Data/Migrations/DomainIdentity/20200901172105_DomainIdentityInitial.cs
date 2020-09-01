using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDennis.AspNetIdentityServer.Data.Migrations.DomainIdentity
{
    public partial class DomainIdentityInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "seqAspNetRoleClaims");

            migrationBuilder.CreateSequence<int>(
                name: "seqAspNetRoles");

            migrationBuilder.CreateSequence<int>(
                name: "seqAspNetUserClaims");

            migrationBuilder.CreateSequence<int>(
                name: "seqAspNetUsers");

            migrationBuilder.CreateTable(
                name: "AspNetApplicationClaims",
                columns: table => new
                {
                    Application = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    ClaimType = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    ClaimValue = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    OrgAdminable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetApplicationClaims", x => new { x.Application, x.ClaimType, x.ClaimValue });
                });

            migrationBuilder.CreateTable(
                name: "AspNetApplications",
                columns: table => new
                {
                    Name = table.Column<string>(unicode: false, maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetApplications", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "AspNetOrganizationApplications",
                columns: table => new
                {
                    Organization = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    Application = table.Column<string>(unicode: false, maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetOrganizationApplications", x => new { x.Organization, x.Application });
                });

            migrationBuilder.CreateTable(
                name: "AspNetOrganizations",
                columns: table => new
                {
                    Name = table.Column<string>(unicode: false, maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetOrganizations", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValueSql: "NEXT VALUE FOR seqAspNetRoles"),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValueSql: "NEXT VALUE FOR seqAspNetUsers"),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Organization = table.Column<string>(unicode: false, maxLength: 128, nullable: true),
                    OrganizationConfirmed = table.Column<bool>(nullable: false),
                    OrganizationAdmin = table.Column<bool>(nullable: false),
                    SuperAdmin = table.Column<bool>(nullable: false),
                    LockoutBegin = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsersHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    DateReplaced = table.Column<DateTime>(nullable: false),
                    ReplacedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    LockoutBegin = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    Organization = table.Column<string>(maxLength: 128, nullable: true),
                    OrganizationConfirmed = table.Column<bool>(nullable: false),
                    OrganizationAdmin = table.Column<bool>(nullable: false),
                    UserClaims = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsersHistory", x => new { x.Id, x.DateReplaced });
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValueSql: "NEXT VALUE FOR seqAspNetRoleClaims"),
                    RoleId = table.Column<int>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValueSql: "NEXT VALUE FOR seqAspNetUserClaims"),
                    UserId = table.Column<int>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    RoleId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetApplicationClaims",
                columns: new[] { "Application", "ClaimType", "ClaimValue", "OrgAdminable" },
                values: new object[,]
                {
                    { "DataGenie", "app:role", "admin", true },
                    { "DataGenie", "app:role", "user", true },
                    { "InfoMaster", "app:role", "admin", true },
                    { "InfoMaster", "app:role", "readonly", true },
                    { "InfoMaster", "app:role", "auditor", false }
                });

            migrationBuilder.InsertData(
                table: "AspNetApplications",
                column: "Name",
                values: new object[]
                {
                    "DataGenie",
                    "InfoMaster"
                });

            migrationBuilder.InsertData(
                table: "AspNetOrganizationApplications",
                columns: new[] { "Organization", "Application" },
                values: new object[,]
                {
                    { "Windy's", "InfoMaster" },
                    { "Burger Squire", "DataGenie" },
                    { "McDougall's", "DataGenie" },
                    { "Burger Squire", "InfoMaster" }
                });

            migrationBuilder.InsertData(
                table: "AspNetOrganizations",
                column: "Name",
                values: new object[]
                {
                    "Burger Squire",
                    "McDougall's",
                    "Windy's"
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutBegin", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "Organization", "OrganizationAdmin", "OrganizationConfirmed", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "SuperAdmin", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { -10, 0, "89e3aefc-48b6-4c00-a2ea-3e5949438e2e", "alice@windys.com", true, null, null, "ALICE@WINDYS.COM", "ALICE@WINDYS.COM", "Windy's", false, false, "ABECH/3HNbsodHmwTd7Di9HAIDvmFKfbWynWnaGOn7wVQNyNvjRuaZC7sCBgUYxs/A==", "222-333-4446", false, "ae61c7b2-80c7-409c-939b-176f24416b8b", false, false, "alice@windys.com" },
                    { -1, 0, "f9db615d-a76c-417b-8c9a-2d7bbfdb2d37", "moe@mcdougalls.com", true, null, null, "MOE@MCDOUGALLS.COM", "MOE@MCDOUGALLS.COM", "McDougall's", true, false, "AHQhhv/JbmFkA5O3OJyamWfdtRAt5eC3YuR9FOwmNHwZsgkGX80SckP/C6uLO6AtBg==", "000-111-2222", false, "8a908948-f7a2-45a4-a2cc-ba4b121d6a62", false, false, "moe@mcdougalls.com" },
                    { -2, 0, "7ee29c84-9b20-4f23-8c4c-1a7ad68a391e", "larry@burgersquire.com", true, null, null, "LARRY@BURGERSQUIRE.COM", "LARRY@BURGERSQUIRE.COM", "Burger Squire", true, false, "ALG1OP/NCWjFpC5rSBSDrauVvPA3EbMxby77Uwt8UKpH58agaCXlnIv1cg4xQE/iHA==", "111-222-3333", true, "42243ffc-d18c-4b22-890e-bd4c519ddcf7", false, false, "larry@burgersquire.com" },
                    { -3, 0, "dcb1bd4e-df97-4f43-a60c-9bedd760709c", "curly@windys.com", true, null, null, "CURLY@WINDYS.COM", "CURLY@WINDYS.COM", "Windy's", true, false, "AEHKZugG3D/te0QUgaQ38qtmApdbvE9TjpS38iG/btctQsuWudaCH3UGTAvv6c5qIA==", "222-333-4444", false, "83d6a4a8-4ca3-4a32-aecc-f36c13dfcb9c", false, false, "curly@windys.com" },
                    { -4, 0, "cd4b8ee6-2df0-4bfe-881e-2ed03b93ed51", "marcia@mcdougalls.com", true, null, null, "MARCIA@MCDOUGALLS.COM", "MARCIA@MCDOUGALLS.COM", "McDougall's", false, false, "AHc7eMal2Ju+KxGC+DNX5ykpx/dPdmhq4z4mPoJ3cEL1BYu78szo+EqmDdmqSpGE4Q==", "000-111-2223", false, "0732f0a2-1ddc-44e1-91bf-eba6a70d0c9a", false, false, "marcia@mcdougalls.com" },
                    { -5, 0, "b64813f6-221c-4a26-8ef8-cf6f380c9db2", "jan@burgersquire.com", true, null, null, "JAN@BURGERSQUIRE.COM", "JAN@BURGERSQUIRE.COM", "Burger Squire", false, false, "AGClgbBglDK154+J/FK9rR0yAzwS8TM029v6bZSEAd/ByzszB4RYa/zBolN34s8F6g==", "111-222-3334", true, "c3d92933-c2fc-48e2-bfa9-82819292f2bf", false, false, "jan@burgersquire.com" },
                    { -6, 0, "dc643442-e6a5-4b13-82ca-687ca7aea13f", "cindy@windys.com", true, null, null, "CINDY@WINDYS.COM", "CINDY@WINDYS.COM", "Windy's", false, false, "AKonnfUXy2u6fFkWm5wH7KCO7EPylRXBRSQy02rw3y+VawwCLj8gy+aUQL/C9Z3e0g==", "222-333-4445", false, "0c2894ae-0f6c-4e23-9c80-9628c8ec38a7", false, false, "cindy@windys.com" },
                    { -7, 0, "88a26bdb-f4c0-46be-98e1-67d9a767dd6f", "greg@mcdougalls.com", true, null, null, "GREG@MCDOUGALLS.COM", "GREG@MCDOUGALLS.COM", "McDougall's", false, false, "APFsitEgUngOUmkzmJX4FcC0QWUD3j3itJ4kGejCqdNR8FMeNfnay53l9wBJbAjlVw==", "000-111-2224", false, "33873d4c-7429-428e-b7fd-04e0b859fef7", false, false, "greg@mcdougalls.com" },
                    { -8, 0, "dd41f3b8-70f7-4e55-92ed-0acb00e64912", "peter@burgersquire.com", true, null, null, "PETER@BURGERSQUIRE.COM", "PETER@BURGERSQUIRE.COM", "Burger Squire", false, false, "APd0egA0D56GUOXi94zvCXi6V6+oiaAeqemgLjiDYYAx6oVpTM/vJZsY/6qykRTZiQ==", "111-222-3335", true, "f71c44eb-51b8-4aed-a92b-a763387e9501", false, false, "peter@burgersquire.com" },
                    { -9, 0, "65ae7b12-27cb-4bde-8bab-416e57414c9d", "bobby@windys.com", true, new DateTimeOffset(new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)), new DateTimeOffset(new DateTime(2030, 12, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)), "BOBBY@WINDYS.COM", "BOBBY@WINDYS.COM", "Windy's", false, false, "ALa6v4WiOqr74SpTWqR1nYCBjV4IffezJ9U1lDsnWcBw4cIQFqNkpiK9M5f6XxSgiQ==", "222-333-4446", false, "6cfffb56-34e9-425f-b4ab-575c66939eb3", false, false, "bobby@windys.com" },
                    { -11, 0, "e242e8fc-d5d0-40fe-b7b8-7dbd570b3167", "sheldon@burgersquire.com", true, null, null, "SHELDON@BURGERSQUIRE.COM", "SHELDON@BURGERSQUIRE.COM", "Burger Squire", false, false, "AGUebo3/plsz2lMKYn7bZ/BIFk3gQvZ5m3rsbGl9SNvZNVyoa6IGJtaPjIpyfOj2tw==", "999-888-7777", false, "3d82bce7-da76-4d86-8605-d2594f80180b", false, false, "sheldon@burgersquire.com" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "UserId" },
                values: new object[,]
                {
                    { -9902, "app:role", "DataGenie:admin", -1 },
                    { -9904, "app:role", "DataGenie:admin", -2 },
                    { -9905, "app:role", "InfoMaster:admin", -2 },
                    { -9907, "app:role", "InfoMaster:admin", -3 },
                    { -9908, "app:role", "DataGenie:user", -4 },
                    { -9909, "app:role", "DataGenie:user", -5 },
                    { -9910, "app:role", "InfoMaster:readonly", -5 },
                    { -9911, "app:role", "InfoMaster:readonly", -6 },
                    { -9912, "app:role", "DataGenie:user", -7 },
                    { -9913, "app:role", "DataGenie:user", -8 },
                    { -9914, "app:role", "InfoMaster:readonly", -8 },
                    { -9915, "app:role", "InfoMaster:readonly", -9 },
                    { -9916, "app:role", "InfoMaster:auditor", -10 },
                    { -9917, "*:role", "*:admin", -11 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.Sql(File.ReadAllText("Data/Sql/ForeignKeys.sql"));
            migrationBuilder.Sql(File.ReadAllText("Data/Sql/SearchableDomainUser.sql"));
            migrationBuilder.Sql(File.ReadAllText("Data/Sql/DbView.sql"));


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetApplicationClaims");

            migrationBuilder.DropTable(
                name: "AspNetApplications");

            migrationBuilder.DropTable(
                name: "AspNetOrganizationApplications");

            migrationBuilder.DropTable(
                name: "AspNetOrganizations");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsersHistory");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropSequence(
                name: "seqAspNetRoleClaims");

            migrationBuilder.DropSequence(
                name: "seqAspNetRoles");

            migrationBuilder.DropSequence(
                name: "seqAspNetUserClaims");

            migrationBuilder.DropSequence(
                name: "seqAspNetUsers");
        }
    }
}
