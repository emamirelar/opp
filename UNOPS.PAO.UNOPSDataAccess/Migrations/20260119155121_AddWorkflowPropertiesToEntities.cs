using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowPropertiesToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_WorkflowStages_WorkflowStageId",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropTable(
                name: "WorkflowLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WorkflowStages",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_WorkflowStageId",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "WorkflowStageId",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "UserProfile",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "UserPreferences",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "Units",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "Risks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "RiskCategories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "ProposedInitiativeTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "ProjectCategories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "PreDefinedHighRisks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "PartnerTrees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "Outputs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OrganizationUnitRelationships",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OrganizationHierarchies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Stage",
                schema: "public",
                table: "Opportunities",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "Opportunities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "Links",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "LiaisonOffices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "Interactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityUserRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityRolePersons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityManagers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityFieldManagers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityArtifacts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "Entities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "DocumentTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "Documents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "DocumentRelationships",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "Contacts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "Comments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "AuditLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "ArtifactTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "ArtifactExtractionRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "ArtifactDataTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "RiskCategories");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "ProposedInitiativeTypes");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "ProjectCategories");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "PreDefinedHighRisks");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "PartnerTrees");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OrganizationUnitRelationships");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OrganizationHierarchies");

            migrationBuilder.DropColumn(
                name: "Stage",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "Links");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "LiaisonOffices");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityUserRoles");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityRoles");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityRolePersons");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityManagers");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityFieldManagers");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "EntityArtifacts");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "DocumentRelationships");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "ArtifactTypes");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "ArtifactExtractionRules");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "ArtifactDataTypes");

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStageId",
                schema: "public",
                table: "Opportunities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkflowLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedBy = table.Column<int>(type: "integer", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NewStage = table.Column<string>(type: "text", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStages",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AllowsParallelProcessing = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedBy = table.Column<int>(type: "integer", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsFinalStage = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_WorkflowStageId",
                schema: "public",
                table: "Opportunities",
                column: "WorkflowStageId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStages_EntityType_Order",
                schema: "public",
                table: "WorkflowStages",
                columns: new[] { "EntityType", "Order" });

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_WorkflowStages_WorkflowStageId",
                schema: "public",
                table: "Opportunities",
                column: "WorkflowStageId",
                principalSchema: "public",
                principalTable: "WorkflowStages",
                principalColumn: "Id");
        }
    }
}
