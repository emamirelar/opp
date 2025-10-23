using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OpportunityId",
                schema: "public",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    JsonData = table.Column<string>(type: "jsonb", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityRoles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsInternal = table.Column<bool>(type: "boolean", nullable: false),
                    AllowsMultiple = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_EntityRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProposedInitiativeTypes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ProposedInitiativeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStages",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    AllowsParallelProcessing = table.Column<bool>(type: "boolean", nullable: false),
                    IsFinalStage = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_WorkflowStages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityRolePersons",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    EntityRoleId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    ContactId = table.Column<int>(type: "integer", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_EntityRolePersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityRolePersons_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntityRolePersons_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalSchema: "public",
                        principalTable: "Contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntityRolePersons_EntityRoles_EntityRoleId",
                        column: x => x.EntityRoleId,
                        principalSchema: "public",
                        principalTable: "EntityRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Opportunities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PartnerReference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    WorkflowStageId = table.Column<int>(type: "integer", nullable: true),
                    ResponsibleOrgUnitId = table.Column<int>(type: "integer", nullable: true),
                    PartnershipAgreementReference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    InitiativeBudgetUSD = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TargetSigningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TargetDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProposedInitiativeTypeId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_Opportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opportunities_OrganizationHierarchies_ResponsibleOrgUnitId",
                        column: x => x.ResponsibleOrgUnitId,
                        principalSchema: "public",
                        principalTable: "OrganizationHierarchies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Opportunities_ProposedInitiativeTypes_ProposedInitiativeTyp~",
                        column: x => x.ProposedInitiativeTypeId,
                        principalSchema: "public",
                        principalTable: "ProposedInitiativeTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Opportunities_WorkflowStages_WorkflowStageId",
                        column: x => x.WorkflowStageId,
                        principalSchema: "public",
                        principalTable: "WorkflowStages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OpportunityClientPartners",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    PartnerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_OpportunityClientPartners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityClientPartners_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityClientPartners_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "public",
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityCountries",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: false),
                    SpecificAreas = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_OpportunityCountries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityCountries_Countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "public",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityCountries_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityDeliverables",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ServiceLineReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_OpportunityDeliverables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityDeliverables_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityFundingPartners",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    PartnerId = table.Column<int>(type: "integer", nullable: false),
                    FundedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    FeePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    FeeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    FeeAmountUSD = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsAmountBasedFee = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_OpportunityFundingPartners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityFundingPartners_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "public",
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityFundingPartners_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityFundingPartners_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "public",
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityStakeholders",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    StakeholderType = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    EntityRoleId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_OpportunityStakeholders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityStakeholders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpportunityStakeholders_EntityRoles_EntityRoleId",
                        column: x => x.EntityRoleId,
                        principalSchema: "public",
                        principalTable: "EntityRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpportunityStakeholders_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_OpportunityId",
                schema: "public",
                table: "Documents",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                schema: "public",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                schema: "public",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_EntityRolePersons_ContactId",
                schema: "public",
                table: "EntityRolePersons",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityRolePersons_EntityRoleId",
                schema: "public",
                table: "EntityRolePersons",
                column: "EntityRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityRolePersons_EntityType_EntityId_EntityRoleId",
                schema: "public",
                table: "EntityRolePersons",
                columns: new[] { "EntityType", "EntityId", "EntityRoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityRolePersons_UserId",
                schema: "public",
                table: "EntityRolePersons",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityRoles_EntityType_Name",
                schema: "public",
                table: "EntityRoles",
                columns: new[] { "EntityType", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_Name",
                schema: "public",
                table: "Opportunities",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_ProposedInitiativeTypeId",
                schema: "public",
                table: "Opportunities",
                column: "ProposedInitiativeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_ResponsibleOrgUnitId",
                schema: "public",
                table: "Opportunities",
                column: "ResponsibleOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_Status",
                schema: "public",
                table: "Opportunities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_WorkflowStageId",
                schema: "public",
                table: "Opportunities",
                column: "WorkflowStageId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityClientPartners_OpportunityId",
                schema: "public",
                table: "OpportunityClientPartners",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityClientPartners_PartnerId",
                schema: "public",
                table: "OpportunityClientPartners",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCountries_CountryId",
                schema: "public",
                table: "OpportunityCountries",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCountries_OpportunityId",
                schema: "public",
                table: "OpportunityCountries",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityDeliverables_OpportunityId",
                schema: "public",
                table: "OpportunityDeliverables",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityFundingPartners_CurrencyId",
                schema: "public",
                table: "OpportunityFundingPartners",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityFundingPartners_OpportunityId",
                schema: "public",
                table: "OpportunityFundingPartners",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityFundingPartners_PartnerId",
                schema: "public",
                table: "OpportunityFundingPartners",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityStakeholders_EntityRoleId",
                schema: "public",
                table: "OpportunityStakeholders",
                column: "EntityRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityStakeholders_OpportunityId",
                schema: "public",
                table: "OpportunityStakeholders",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityStakeholders_UserId",
                schema: "public",
                table: "OpportunityStakeholders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStages_EntityType_Order",
                schema: "public",
                table: "WorkflowStages",
                columns: new[] { "EntityType", "Order" });

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Opportunities_OpportunityId",
                schema: "public",
                table: "Documents",
                column: "OpportunityId",
                principalSchema: "public",
                principalTable: "Opportunities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Opportunities_OpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EntityRolePersons",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OpportunityClientPartners",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OpportunityCountries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OpportunityDeliverables",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OpportunityFundingPartners",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OpportunityStakeholders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EntityRoles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Opportunities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ProposedInitiativeTypes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WorkflowStages",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Documents_OpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "OpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                schema: "public",
                table: "AiChatSession",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
