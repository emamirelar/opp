using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PartnerAgreementModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedPartnerAgreementNumber",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedPartnerAgreementNumber",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartnerAgreements",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    BasePartnerAgreementNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PartnerAgreementNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PartnerAgreementDescriptionLong = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PartnerAgreementType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PartnerAgreementTypeDescription = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PartnerAgreementScope = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PartnerAgreementScopeDescription = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PartnerAgreementPartner = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PartnerAgreementPartnerDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PartnerAgreementStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PartnerAgreementEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PartnerAgreementSignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PartnerAgreementResponsibleOrgUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PartnerAgreementResponsibleOrgUnitDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PartnerAgreementServiceLineInfrastructureFlag = table.Column<bool>(type: "boolean", nullable: false),
                    PartnerAgreementServiceLineProcurementFlag = table.Column<bool>(type: "boolean", nullable: false),
                    PartnerAgreementServiceLineProjectManagementFlag = table.Column<bool>(type: "boolean", nullable: false),
                    PartnerAgreementServiceLineFundManagementFlag = table.Column<bool>(type: "boolean", nullable: false),
                    PartnerAgreementServiceLineHumanResourcesFlag = table.Column<bool>(type: "boolean", nullable: false),
                    PartnerAgreementServiceLineOtherFlag = table.Column<bool>(type: "boolean", nullable: false),
                    PartnerAgreementCountries = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerAgreements", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerAgreements",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "SelectedPartnerAgreementNumber",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "SelectedPartnerAgreementNumber",
                schema: "public",
                table: "OpportunityClientPartners");
        }
    }
}
