using System;
using Microsoft.EntityFrameworkCore.Migrations;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityModelRefinementUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Outputs_ProjectCategories_ProjectCategoryId",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropForeignKey(
                name: "FK_Outputs_Units_UnitId",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropIndex(
                name: "IX_Outputs_ProjectCategoryId",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropIndex(
                name: "IX_Outputs_UnitId",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "OutputGroup",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "OutputServiceLine",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "ProjectCategoryId",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "public",
                table: "Outputs");

            migrationBuilder.RenameColumn(
                name: "OutputSubGroup",
                schema: "public",
                table: "Outputs",
                newName: "ServiceLine");

            migrationBuilder.RenameColumn(
                name: "OutputName",
                schema: "public",
                table: "Outputs",
                newName: "Level4");

            migrationBuilder.AddColumn<string>(
                name: "DefinitionLevel1",
                schema: "public",
                table: "Outputs",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefinitionLevel2",
                schema: "public",
                table: "Outputs",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefinitionLevel3",
                schema: "public",
                table: "Outputs",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefinitionLevel4",
                schema: "public",
                table: "Outputs",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GrantSupportComponent",
                schema: "public",
                table: "Outputs",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GrantSupportImplementingModality",
                schema: "public",
                table: "Outputs",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InfrastructureComponent",
                schema: "public",
                table: "Outputs",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level0",
                schema: "public",
                table: "Outputs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level1",
                schema: "public",
                table: "Outputs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level2",
                schema: "public",
                table: "Outputs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level3",
                schema: "public",
                table: "Outputs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ProcurementComponent",
                schema: "public",
                table: "Outputs",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ProcurementInstallationComponent",
                schema: "public",
                table: "Outputs",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedEndDate",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStartDate",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SequenceOrder",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "Opportunities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "DeliveryModality",
                schema: "public",
                table: "Opportunities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                schema: "public",
                table: "EntityEmbeddings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                schema: "public",
                table: "EntityEmbeddings",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
            {
                "retrieve_hybrid_search_outputs.sql"
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefinitionLevel1",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "DefinitionLevel2",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "DefinitionLevel3",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "DefinitionLevel4",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "GrantSupportComponent",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "GrantSupportImplementingModality",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "InfrastructureComponent",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "Level0",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "Level1",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "Level2",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "Level3",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "ProcurementComponent",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "ProcurementInstallationComponent",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropColumn(
                name: "PlannedEndDate",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "PlannedStartDate",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "SequenceOrder",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "DeliveryModality",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Keywords",
                schema: "public",
                table: "EntityEmbeddings");

            migrationBuilder.DropColumn(
                name: "Metadata",
                schema: "public",
                table: "EntityEmbeddings");

            migrationBuilder.RenameColumn(
                name: "ServiceLine",
                schema: "public",
                table: "Outputs",
                newName: "OutputSubGroup");

            migrationBuilder.RenameColumn(
                name: "Level4",
                schema: "public",
                table: "Outputs",
                newName: "OutputName");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "Outputs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutputGroup",
                schema: "public",
                table: "Outputs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutputServiceLine",
                schema: "public",
                table: "Outputs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectCategoryId",
                schema: "public",
                table: "Outputs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "public",
                table: "Outputs",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "Opportunities",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "IX_Outputs_ProjectCategoryId",
                schema: "public",
                table: "Outputs",
                column: "ProjectCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Outputs_UnitId",
                schema: "public",
                table: "Outputs",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Outputs_ProjectCategories_ProjectCategoryId",
                schema: "public",
                table: "Outputs",
                column: "ProjectCategoryId",
                principalSchema: "public",
                principalTable: "ProjectCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Outputs_Units_UnitId",
                schema: "public",
                table: "Outputs",
                column: "UnitId",
                principalSchema: "public",
                principalTable: "Units",
                principalColumn: "Id");
        }
    }
}
