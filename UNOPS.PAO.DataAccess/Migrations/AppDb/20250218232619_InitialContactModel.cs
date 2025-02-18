using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.DataAccess.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class InitialContactModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmissionDueDate",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "InformationSessionDate",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DecisionDate",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClarificationDeadline",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "Contacts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Salutation = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    MiddleName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Suffix = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Pronouns = table.Column<string>(type: "text", nullable: false),
                    Department = table.Column<string>(type: "text", nullable: false),
                    Birthdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Fax = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Mobile = table.Column<string>(type: "text", nullable: false),
                    OtherPhone = table.Column<string>(type: "text", nullable: false),
                    Assistant = table.Column<string>(type: "text", nullable: false),
                    AssistantPhone = table.Column<string>(type: "text", nullable: false),
                    AssistantEmail = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: true),
                    MailingStreet = table.Column<string>(type: "text", nullable: false),
                    MailingStreet2 = table.Column<string>(type: "text", nullable: false),
                    MailingCity = table.Column<string>(type: "text", nullable: false),
                    MailingStateProvince = table.Column<string>(type: "text", nullable: false),
                    MailingPostalCode = table.Column<string>(type: "text", nullable: false),
                    MailingCountry = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Iso2Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityUserRoles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_EntityUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityUserRoles_AspNetUserRoles_UserId_RoleId",
                        columns: x => new { x.UserId, x.RoleId },
                        principalSchema: "public",
                        principalTable: "AspNetUserRoles",
                        principalColumns: new[] { "UserId", "RoleId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SDGs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    LongDescription = table.Column<string>(type: "text", nullable: false),
                    Logo = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDGs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundingOpportunityCountries",
                schema: "public",
                columns: table => new
                {
                    CountriesId = table.Column<int>(type: "integer", nullable: false),
                    FundingOpportunityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingOpportunityCountries", x => new { x.CountriesId, x.FundingOpportunityId });
                    table.ForeignKey(
                        name: "FK_FundingOpportunityCountries_Countries_CountriesId",
                        column: x => x.CountriesId,
                        principalSchema: "public",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingOpportunityCountries_FundingOpportunities_FundingOpp~",
                        column: x => x.FundingOpportunityId,
                        principalSchema: "public",
                        principalTable: "FundingOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundingOpportunitySDGs",
                schema: "public",
                columns: table => new
                {
                    FundingOpportunityId = table.Column<int>(type: "integer", nullable: false),
                    SDGsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingOpportunitySDGs", x => new { x.FundingOpportunityId, x.SDGsId });
                    table.ForeignKey(
                        name: "FK_FundingOpportunitySDGs_FundingOpportunities_FundingOpportun~",
                        column: x => x.FundingOpportunityId,
                        principalSchema: "public",
                        principalTable: "FundingOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingOpportunitySDGs_SDGs_SDGsId",
                        column: x => x.SDGsId,
                        principalSchema: "public",
                        principalTable: "SDGs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityUserRoles_UserId_RoleId",
                schema: "public",
                table: "EntityUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunityCountries_FundingOpportunityId",
                schema: "public",
                table: "FundingOpportunityCountries",
                column: "FundingOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunitySDGs_SDGsId",
                schema: "public",
                table: "FundingOpportunitySDGs",
                column: "SDGsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contacts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EntityUserRoles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FundingOpportunityCountries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FundingOpportunitySDGs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Countries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SDGs",
                schema: "public");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmissionDueDate",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "InformationSessionDate",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DecisionDate",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClarificationDeadline",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
