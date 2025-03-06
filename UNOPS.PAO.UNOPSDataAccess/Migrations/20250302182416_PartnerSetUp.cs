using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PartnerSetUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                schema: "public",
                table: "Contacts",
                type: "integer",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "Partners",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    NewEngagement = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    Address1Street = table.Column<string>(type: "text", nullable: true),
                    Address1Street2 = table.Column<string>(type: "text", nullable: true),
                    Address1City = table.Column<string>(type: "text", nullable: true),
                    Address1StateProvince = table.Column<string>(type: "text", nullable: true),
                    Address1PostalCode = table.Column<string>(type: "text", nullable: true),
                    Address1Country = table.Column<string>(type: "text", nullable: true),
                    Address2Street = table.Column<string>(type: "text", nullable: true),
                    Address2Street2 = table.Column<string>(type: "text", nullable: true),
                    Address2City = table.Column<string>(type: "text", nullable: true),
                    Address2StateProvince = table.Column<string>(type: "text", nullable: true),
                    Address2PostalCode = table.Column<string>(type: "text", nullable: true),
                    Address2Country = table.Column<string>(type: "text", nullable: true),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    InternalReportingLevel = table.Column<string>(type: "text", nullable: true),
                    ExternalReportingLevel = table.Column<string>(type: "text", nullable: true),
                    PooledFund = table.Column<string>(type: "text", nullable: false),
                    DDRequired = table.Column<string>(type: "text", nullable: false),
                    DDEACDone = table.Column<string>(type: "text", nullable: false),
                    EACReference = table.Column<string>(type: "text", nullable: true),
                    GlobalKeyAccount = table.Column<bool>(type: "boolean", nullable: false),
                    UNSecretariatEntity = table.Column<bool>(type: "boolean", nullable: false),
                    LevyPotentiallyApplies = table.Column<string>(type: "text", nullable: false),
                    ReasonForLevyNotApplying = table.Column<string>(type: "text", nullable: true),
                    LevyTreatment = table.Column<string>(type: "text", nullable: true),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
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
                    table.PrimaryKey("PK_Partners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_PartnerId",
                schema: "public",
                table: "Contacts",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Partners_PartnerId",
                schema: "public",
                table: "Contacts",
                column: "PartnerId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Partners_PartnerId",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropTable(
                name: "Partners",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_PartnerId",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                schema: "public",
                table: "Contacts");
        }
    }
}
