using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OrgHierarchiesModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interactions_OrganizationUnits_OrgUnitId",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Partners_OrganizationUnits_PartnerOfficeId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropTable(
                name: "OrganizationUnits",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "OrganizationHierarchies",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<int>(type: "integer", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationHierarchies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationHierarchies_OrganizationHierarchies_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "public",
                        principalTable: "OrganizationHierarchies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationHierarchies_ParentId",
                schema: "public",
                table: "OrganizationHierarchies",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interactions_OrganizationHierarchies_OrgUnitId",
                schema: "public",
                table: "Interactions",
                column: "OrgUnitId",
                principalSchema: "public",
                principalTable: "OrganizationHierarchies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_OrganizationHierarchies_PartnerOfficeId",
                schema: "public",
                table: "Partners",
                column: "PartnerOfficeId",
                principalSchema: "public",
                principalTable: "OrganizationHierarchies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interactions_OrganizationHierarchies_OrgUnitId",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Partners_OrganizationHierarchies_PartnerOfficeId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropTable(
                name: "OrganizationHierarchies",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "OrganizationUnits",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedBy = table.Column<int>(type: "integer", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnits", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Interactions_OrganizationUnits_OrgUnitId",
                schema: "public",
                table: "Interactions",
                column: "OrgUnitId",
                principalSchema: "public",
                principalTable: "OrganizationUnits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_OrganizationUnits_PartnerOfficeId",
                schema: "public",
                table: "Partners",
                column: "PartnerOfficeId",
                principalSchema: "public",
                principalTable: "OrganizationUnits",
                principalColumn: "Id");
        }
    }
}
