using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityChildTableModels1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExchangeRates_Exchange_Rate_End_Date",
                schema: "public",
                table: "ExchangeRates");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeRates_Exchange_Rate_Start_Date",
                schema: "public",
                table: "ExchangeRates");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeRates_Is_Current_Flag",
                schema: "public",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "Currency_Description",
                schema: "public",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "Currency_Type",
                schema: "public",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "Exchange_Rate_End_Date",
                schema: "public",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "Exchange_Rate_Line_Source",
                schema: "public",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "Exchange_Rate_Start_Date",
                schema: "public",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "Is_Current_Flag",
                schema: "public",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "Rate_Expiration",
                schema: "public",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "Registered_Rate",
                schema: "public",
                table: "ExchangeRates");

            migrationBuilder.AddColumn<string>(
                name: "ContinentDescription",
                schema: "public",
                table: "Countries",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionDescription",
                schema: "public",
                table: "Countries",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_ContinentDescription",
                schema: "public",
                table: "Countries",
                column: "ContinentDescription");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_RegionDescription",
                schema: "public",
                table: "Countries",
                column: "RegionDescription");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Countries_ContinentDescription",
                schema: "public",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Countries_RegionDescription",
                schema: "public",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "ContinentDescription",
                schema: "public",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "RegionDescription",
                schema: "public",
                table: "Countries");

            migrationBuilder.AddColumn<string>(
                name: "Currency_Description",
                schema: "public",
                table: "ExchangeRates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency_Type",
                schema: "public",
                table: "ExchangeRates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Exchange_Rate_End_Date",
                schema: "public",
                table: "ExchangeRates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Exchange_Rate_Line_Source",
                schema: "public",
                table: "ExchangeRates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Exchange_Rate_Start_Date",
                schema: "public",
                table: "ExchangeRates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Is_Current_Flag",
                schema: "public",
                table: "ExchangeRates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Rate_Expiration",
                schema: "public",
                table: "ExchangeRates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Registered_Rate",
                schema: "public",
                table: "ExchangeRates",
                type: "numeric(18,8)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Exchange_Rate_End_Date",
                schema: "public",
                table: "ExchangeRates",
                column: "Exchange_Rate_End_Date");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Exchange_Rate_Start_Date",
                schema: "public",
                table: "ExchangeRates",
                column: "Exchange_Rate_Start_Date");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Is_Current_Flag",
                schema: "public",
                table: "ExchangeRates",
                column: "Is_Current_Flag");
        }
    }
}
