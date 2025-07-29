using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OrgUnitDynamicManytoManyRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partners_OrganizationHierarchies_PartnerOfficeId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_PartnerOfficeId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerOfficeId",
                schema: "public",
                table: "Partners");

            migrationBuilder.CreateTable(
                name: "OrganizationUnitRelationships",
                schema: "public",
                columns: table => new
                {
                    OrganizationHierarchyId = table.Column<int>(type: "integer", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_OrganizationUnitRelationships", x => new { x.OrganizationHierarchyId, x.EntityId, x.EntityType });
                    table.ForeignKey(
                        name: "FK_OrganizationUnitRelationships_OrganizationHierarchies_Organ~",
                        column: x => x.OrganizationHierarchyId,
                        principalSchema: "public",
                        principalTable: "OrganizationHierarchies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitRelationships_Partners_EntityId",
                        column: x => x.EntityId,
                        principalSchema: "public",
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRelationships_EntityId_EntityType",
                schema: "public",
                table: "OrganizationUnitRelationships",
                columns: new[] { "EntityId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRelationships_EntityId_EntityType_Organizat~",
                schema: "public",
                table: "OrganizationUnitRelationships",
                columns: new[] { "EntityId", "EntityType", "OrganizationHierarchyId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationUnitRelationships",
                schema: "public");

            migrationBuilder.AddColumn<int>(
                name: "PartnerOfficeId",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerOfficeId",
                schema: "public",
                table: "Partners",
                column: "PartnerOfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_OrganizationHierarchies_PartnerOfficeId",
                schema: "public",
                table: "Partners",
                column: "PartnerOfficeId",
                principalSchema: "public",
                principalTable: "OrganizationHierarchies",
                principalColumn: "Id");
        }
    }
}
