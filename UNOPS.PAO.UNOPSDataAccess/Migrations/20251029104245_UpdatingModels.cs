using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityClientPartners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunitySDGs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunitySDGs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunitySDGs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunitySDGs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunitySDGs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunitySDGs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunitySDGs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunitySDGs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunitySDGs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityCountries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityCountries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityCountries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityCountries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityCountries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityCountries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityCountries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityCountries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityCountries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
