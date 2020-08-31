using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDennis.AspNetIdentityServer.Data.Migrations.DomainIdentity
{
    public partial class Initial : Migration
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
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutBegin", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "Organization", "OrganizationAdmin", "OrganizationConfirmed", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { -10, 0, "ca9342ca-50a3-41fc-a867-300abe024ff6", "alice@windys.com", true, null, null, "ALICE@WINDYS.COM", "ALICE@WINDYS.COM", "Windy's", false, false, "AHfTD2iqsjzwV3zBRIwUFCg27J/9L391ShpGQMQD/sJnVf+62mhAJgZaHDDIdtggzw==", "222-333-4446", false, "cce2a95d-ceb9-4451-864b-8311e4144c10", false, "alice@windys.com" },
                    { -1, 0, "4c89a4a5-53ed-4e69-b9d6-ec192198e8ff", "moe@mcdougalls.com", true, null, null, "MOE@MCDOUGALLS.COM", "MOE@MCDOUGALLS.COM", "McDougall's", true, false, "AA9h108SlVBssRSc+Dqntg7bZiwt6AQPEsxZOhY7pcUGDy3v4fRsEDEVhNJk4rr43A==", "000-111-2222", false, "e390994a-b411-44de-a3a7-d59bfbb9cf5a", false, "moe@mcdougalls.com" },
                    { -2, 0, "22a140fa-b110-42fc-b9d8-e6dcbc3ea43f", "larry@burgersquire.com", true, null, null, "LARRY@BURGERSQUIRE.COM", "LARRY@BURGERSQUIRE.COM", "Burger Squire", true, false, "AH56YEMKGpGxvsomtHRDBmKQSu/NyD04OEJSY49Llq9PL+t2CHoWoMmqCrSvZBujig==", "111-222-3333", true, "0d748aed-63d4-431d-beec-e590e1f841e3", false, "larry@burgersquire.com" },
                    { -3, 0, "4414f7aa-27f3-4cc0-a033-177009989968", "curly@windys.com", true, null, null, "CURLY@WINDYS.COM", "CURLY@WINDYS.COM", "Windy's", true, false, "AEsOfG0oSS+J7tUfteSOMknWfYoZv29wZv6Fzo3BEDBBnwHHLJDP4SA82+dw28HrIA==", "222-333-4444", false, "8013a9af-6639-4a98-bf12-f6e442e898c6", false, "curly@windys.com" },
                    { -4, 0, "6f72b5b1-b143-4168-885e-9aa0857b7482", "marcia@mcdougalls.com", true, null, null, "MARCIA@MCDOUGALLS.COM", "MARCIA@MCDOUGALLS.COM", "McDougall's", false, false, "AKJy5CVEFbEto2XHI6Nv07QMsOWaKMYOuNuLo5k5l8yG6HmFFyqJ+GexdR38ErrpoQ==", "000-111-2223", false, "1b6fba77-42f4-4585-b8c7-2689a672d97e", false, "marcia@mcdougalls.com" },
                    { -5, 0, "0759e9c5-237c-4e8a-be87-dc84d077d3c1", "jan@burgersquire.com", true, null, null, "JAN@BURGERSQUIRE.COM", "JAN@BURGERSQUIRE.COM", "Burger Squire", false, false, "AOKFZ/0fs5ZlFlgBcdRs0/GAtr54YIc9X8zv6YMez9UIR7DtVZW608tRa4j/LU8t8w==", "111-222-3334", true, "73bc5052-a491-4eef-833e-e5cef8e7f2d9", false, "jan@burgersquire.com" },
                    { -6, 0, "6aa8809e-9ecb-40bb-a7c8-99ea2a3c6ac9", "cindy@windys.com", true, null, null, "CINDY@WINDYS.COM", "CINDY@WINDYS.COM", "Windy's", false, false, "AFexRbApNOVdhRaK/fRolsFxeRkw1boJ9/QUqhkeIX54brYEp+IvuV05ssGkdqs8Dg==", "222-333-4445", false, "60303360-ff3b-4d79-b560-a0ca49900067", false, "cindy@windys.com" },
                    { -7, 0, "5a6fdbb9-e7cc-4808-9bb0-57e602bc148e", "greg@mcdougalls.com", true, null, null, "GREG@MCDOUGALLS.COM", "GREG@MCDOUGALLS.COM", "McDougall's", false, false, "AOs5pcwsec1Fr0ctoOa4zGGDYJcWJxd1DwEvbsEHwyYm+/dxgxbxsaE8EwsdErS+pQ==", "000-111-2224", false, "502a028d-947b-4501-bf53-14e591a29bb3", false, "greg@mcdougalls.com" },
                    { -8, 0, "1d494787-9bd6-4018-be6c-c88398708cd0", "peter@burgersquire.com", true, null, null, "PETER@BURGERSQUIRE.COM", "PETER@BURGERSQUIRE.COM", "Burger Squire", false, false, "AOVUIcfSFifkaIYM25VzeT5lI6Ra2HlVMzdYNCIaB8Hkxjs8vO5N+m/N4A6YrjyvGw==", "111-222-3335", true, "5401e644-4b04-46d2-ab38-ce99ab05fe48", false, "peter@burgersquire.com" },
                    { -9, 0, "2c4ca8aa-bf3e-44f2-995a-57ff1f4ad291", "bobby@windys.com", true, new DateTimeOffset(new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)), new DateTimeOffset(new DateTime(2030, 12, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -5, 0, 0, 0)), "BOBBY@WINDYS.COM", "BOBBY@WINDYS.COM", "Windy's", false, false, "ALLUTZJjhrofh2bxvJMxX5wr844YB2A1TpRrFsInH/sqtoI4KeXCdOloxmmO2aXTQw==", "222-333-4446", false, "f9b2716a-bb24-445e-b333-420f88001e35", false, "bobby@windys.com" },
                    { -11, 0, "94e81422-7d0a-447e-9c76-e2b8ab7d7413", "sheldon@burgersquire.com", true, null, null, "SHELDON@BURGERSQUIRE.COM", "SHELDON@BURGERSQUIRE.COM", "Burger Squire", false, false, "ALQmLyWUPXDt1DZ1Xtp3feAosNKQKe6WUqalvcsVN7WrcHtCmrB5MZCylLlo/KZBWg==", "999-888-7777", false, "a7d1aaec-0d39-4dbe-8804-1e97d52b0374", false, "sheldon@burgersquire.com" }
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
