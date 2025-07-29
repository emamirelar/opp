using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OrgUnitManyToManyRemovePartnerIdFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUnitRelationships_Partners_EntityId",
                schema: "public",
                table: "OrganizationUnitRelationships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrganizationUnitRelationships",
                schema: "public",
                table: "OrganizationUnitRelationships");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "public",
                table: "OrganizationUnitRelationships",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrganizationUnitRelationships",
                schema: "public",
                table: "OrganizationUnitRelationships",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRelationships_OrganizationHierarchyId",
                schema: "public",
                table: "OrganizationUnitRelationships",
                column: "OrganizationHierarchyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrganizationUnitRelationships",
                schema: "public",
                table: "OrganizationUnitRelationships");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUnitRelationships_OrganizationHierarchyId",
                schema: "public",
                table: "OrganizationUnitRelationships");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "public",
                table: "OrganizationUnitRelationships",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrganizationUnitRelationships",
                schema: "public",
                table: "OrganizationUnitRelationships",
                columns: new[] { "OrganizationHierarchyId", "EntityId", "EntityType" });

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUnitRelationships_Partners_EntityId",
                schema: "public",
                table: "OrganizationUnitRelationships",
                column: "EntityId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
