using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedGrantPlusEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_FundingOpportunities_FundingOpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Proposals_ProposalId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "FundingOpportunityCountries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FundingOpportunityEligibleEntities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FundingOpportunitySDGs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Proposals",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SDGs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FundingOpportunities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SelectionMethodologies",
                schema: "public");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Projects_ProjectNumber",
                schema: "public",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Documents_FundingOpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ProposalId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FundingOpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ProposalId",
                schema: "public",
                table: "Documents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FundingOpportunityId",
                schema: "public",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProposalId",
                schema: "public",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Projects_ProjectNumber",
                schema: "public",
                table: "Projects",
                column: "ProjectNumber");

            migrationBuilder.CreateTable(
                name: "SDGs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Logo = table.Column<string>(type: "text", nullable: false),
                    LongDescription = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Number = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDGs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SelectionMethodologies",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedBy = table.Column<int>(type: "integer", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectionMethodologies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundingOpportunities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrencyId = table.Column<int>(type: "integer", nullable: true),
                    SelectionMethodologyId = table.Column<int>(type: "integer", nullable: true),
                    ApplicationTypeCode = table.Column<string>(type: "text", nullable: true),
                    ClarificationDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<int>(type: "integer", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    EligibilityCriteria = table.Column<string>(type: "text", nullable: false),
                    FundingAvailable = table.Column<decimal>(type: "numeric", nullable: false),
                    InformationSessionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Justification = table.Column<string>(type: "text", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SingleSubmition = table.Column<bool>(type: "boolean", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmissionDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectNumber = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingOpportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundingOpportunities_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "public",
                        principalTable: "Currencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FundingOpportunities_Projects_ProjectNumber",
                        column: x => x.ProjectNumber,
                        principalSchema: "public",
                        principalTable: "Projects",
                        principalColumn: "ProjectNumber");
                    table.ForeignKey(
                        name: "FK_FundingOpportunities_SelectionMethodologies_SelectionMethod~",
                        column: x => x.SelectionMethodologyId,
                        principalSchema: "public",
                        principalTable: "SelectionMethodologies",
                        principalColumn: "Id");
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
                name: "FundingOpportunityEligibleEntities",
                schema: "public",
                columns: table => new
                {
                    EligibleEntitiesId = table.Column<int>(type: "integer", nullable: false),
                    FundingOpportunityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingOpportunityEligibleEntities", x => new { x.EligibleEntitiesId, x.FundingOpportunityId });
                    table.ForeignKey(
                        name: "FK_FundingOpportunityEligibleEntities_EligibleEntities_Eligibl~",
                        column: x => x.EligibleEntitiesId,
                        principalSchema: "public",
                        principalTable: "EligibleEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingOpportunityEligibleEntities_FundingOpportunities_Fun~",
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

            migrationBuilder.CreateTable(
                name: "Proposals",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicantId = table.Column<int>(type: "integer", nullable: false),
                    FundingOpportunityId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedBy = table.Column<int>(type: "integer", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EligibilityCriteriaMet = table.Column<bool>(type: "boolean", nullable: false),
                    EligibilityEntityMet = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proposals_AspNetUsers_ApplicantId",
                        column: x => x.ApplicantId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Proposals_FundingOpportunities_FundingOpportunityId",
                        column: x => x.FundingOpportunityId,
                        principalSchema: "public",
                        principalTable: "FundingOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FundingOpportunityId",
                schema: "public",
                table: "Documents",
                column: "FundingOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ProposalId",
                schema: "public",
                table: "Documents",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunities_CurrencyId",
                schema: "public",
                table: "FundingOpportunities",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunities_ProjectNumber",
                schema: "public",
                table: "FundingOpportunities",
                column: "ProjectNumber");

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunities_SelectionMethodologyId",
                schema: "public",
                table: "FundingOpportunities",
                column: "SelectionMethodologyId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunityCountries_FundingOpportunityId",
                schema: "public",
                table: "FundingOpportunityCountries",
                column: "FundingOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunityEligibleEntities_FundingOpportunityId",
                schema: "public",
                table: "FundingOpportunityEligibleEntities",
                column: "FundingOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunitySDGs_SDGsId",
                schema: "public",
                table: "FundingOpportunitySDGs",
                column: "SDGsId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ApplicantId",
                schema: "public",
                table: "Proposals",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_FundingOpportunityId",
                schema: "public",
                table: "Proposals",
                column: "FundingOpportunityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_FundingOpportunities_FundingOpportunityId",
                schema: "public",
                table: "Documents",
                column: "FundingOpportunityId",
                principalSchema: "public",
                principalTable: "FundingOpportunities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Proposals_ProposalId",
                schema: "public",
                table: "Documents",
                column: "ProposalId",
                principalSchema: "public",
                principalTable: "Proposals",
                principalColumn: "Id");
        }
    }
}
