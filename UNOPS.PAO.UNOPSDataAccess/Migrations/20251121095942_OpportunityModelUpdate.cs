using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountUSD",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "numeric(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExchangeRateDate",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExchangeRateId",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerPreferredCurrency",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityFundingPartners_ExchangeRateId",
                schema: "public",
                table: "OpportunityFundingPartners",
                column: "ExchangeRateId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityFundingPartners_ExchangeRates_ExchangeRateId",
                schema: "public",
                table: "OpportunityFundingPartners",
                column: "ExchangeRateId",
                principalSchema: "public",
                principalTable: "ExchangeRates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityFundingPartners_ExchangeRates_ExchangeRateId",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityFundingPartners_ExchangeRateId",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "AmountUSD",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "ExchangeRateDate",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "ExchangeRateId",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "PartnerPreferredCurrency",
                schema: "public",
                table: "OpportunityFundingPartners");
        }
    }
}
