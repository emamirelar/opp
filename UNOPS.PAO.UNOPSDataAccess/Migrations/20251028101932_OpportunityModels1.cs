using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityModels1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OpportunitySDGs_AlignmentType",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "AlignmentType",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.RenameColumn(
                name: "AlignmentNotes",
                schema: "public",
                table: "OpportunitySDGs",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "FundedAmount",
                schema: "public",
                table: "OpportunityFundingPartners",
                newName: "Amount");

            migrationBuilder.AlterColumn<string>(
                name: "StakeholderType",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "EntityRoleId",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInternal",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Organization",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                schema: "public",
                table: "OpportunitySDGs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CommitmentStatus",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnershipAgreementReference",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Percentage",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceLine",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpecificAreas",
                schema: "public",
                table: "OpportunityCountries",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContextWarning",
                schema: "public",
                table: "OpportunityCountries",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RiskScore",
                schema: "public",
                table: "OpportunityCountries",
                type: "numeric(3,1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedBeneficiaries",
                schema: "public",
                table: "Opportunities",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntendedImpactOutcomes",
                schema: "public",
                table: "Opportunities",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultsFocus",
                schema: "public",
                table: "Opportunities",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrategicAlignment",
                schema: "public",
                table: "Opportunities",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityStakeholders_ContactId",
                schema: "public",
                table: "OpportunityStakeholders",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityStakeholders_Contacts_ContactId",
                schema: "public",
                table: "OpportunityStakeholders",
                column: "ContactId",
                principalSchema: "public",
                principalTable: "Contacts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityStakeholders_Contacts_ContactId",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityStakeholders_ContactId",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "ContactId",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "IsInternal",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "Organization",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "CommitmentStatus",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "PartnershipAgreementReference",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "Percentage",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "ServiceLine",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "ContextWarning",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "RiskScore",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "ExpectedBeneficiaries",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "IntendedImpactOutcomes",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ResultsFocus",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "StrategicAlignment",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.RenameColumn(
                name: "Notes",
                schema: "public",
                table: "OpportunitySDGs",
                newName: "AlignmentNotes");

            migrationBuilder.RenameColumn(
                name: "Amount",
                schema: "public",
                table: "OpportunityFundingPartners",
                newName: "FundedAmount");

            migrationBuilder.AlterColumn<string>(
                name: "StakeholderType",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EntityRoleId",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "AlignmentType",
                schema: "public",
                table: "OpportunitySDGs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpecificAreas",
                schema: "public",
                table: "OpportunityCountries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunitySDGs_AlignmentType",
                schema: "public",
                table: "OpportunitySDGs",
                column: "AlignmentType");
        }
    }
}
