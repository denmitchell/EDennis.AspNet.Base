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
                    ClaimTypePrefix = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    ClaimValue = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    OrgAdminable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetApplicationClaims", x => new { x.Application, x.ClaimTypePrefix, x.ClaimValue });
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
                    Name = table.Column<string>(unicode: false, maxLength: 128, nullable: false),
                    SharedEmail = table.Column<string>(nullable: true)
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
