using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityModels3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityDeliverables_Output_OutputId",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropForeignKey(
                name: "FK_Output_ProjectCategory_ProjectCategoryId",
                schema: "public",
                table: "Output");

            migrationBuilder.DropForeignKey(
                name: "FK_Output_Unit_UnitId",
                schema: "public",
                table: "Output");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Unit",
                schema: "public",
                table: "Unit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectCategory",
                schema: "public",
                table: "ProjectCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Output",
                schema: "public",
                table: "Output");

            migrationBuilder.RenameTable(
                name: "Unit",
                schema: "public",
                newName: "Units",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ProjectCategory",
                schema: "public",
                newName: "ProjectCategories",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Output",
                schema: "public",
                newName: "Outputs",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_Output_UnitId",
                schema: "public",
                table: "Outputs",
                newName: "IX_Outputs_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Output_ProjectCategoryId",
                schema: "public",
                table: "Outputs",
                newName: "IX_Outputs_ProjectCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Units",
                schema: "public",
                table: "Units",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectCategories",
                schema: "public",
                table: "ProjectCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Outputs",
                schema: "public",
                table: "Outputs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityDeliverables_Outputs_OutputId",
                schema: "public",
                table: "OpportunityDeliverables",
                column: "OutputId",
                principalSchema: "public",
                principalTable: "Outputs",
                principalColumn: "Id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityDeliverables_Outputs_OutputId",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropForeignKey(
                name: "FK_Outputs_ProjectCategories_ProjectCategoryId",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropForeignKey(
                name: "FK_Outputs_Units_UnitId",
                schema: "public",
                table: "Outputs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Units",
                schema: "public",
                table: "Units");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectCategories",
                schema: "public",
                table: "ProjectCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Outputs",
                schema: "public",
                table: "Outputs");

            migrationBuilder.RenameTable(
                name: "Units",
                schema: "public",
                newName: "Unit",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ProjectCategories",
                schema: "public",
                newName: "ProjectCategory",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Outputs",
                schema: "public",
                newName: "Output",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_Outputs_UnitId",
                schema: "public",
                table: "Output",
                newName: "IX_Output_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Outputs_ProjectCategoryId",
                schema: "public",
                table: "Output",
                newName: "IX_Output_ProjectCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Unit",
                schema: "public",
                table: "Unit",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectCategory",
                schema: "public",
                table: "ProjectCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Output",
                schema: "public",
                table: "Output",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityDeliverables_Output_OutputId",
                schema: "public",
                table: "OpportunityDeliverables",
                column: "OutputId",
                principalSchema: "public",
                principalTable: "Output",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Output_ProjectCategory_ProjectCategoryId",
                schema: "public",
                table: "Output",
                column: "ProjectCategoryId",
                principalSchema: "public",
                principalTable: "ProjectCategory",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Output_Unit_UnitId",
                schema: "public",
                table: "Output",
                column: "UnitId",
                principalSchema: "public",
                principalTable: "Unit",
                principalColumn: "Id");
        }
    }
}
