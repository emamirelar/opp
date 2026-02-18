using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.DataAccess.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class IgnoreUserProfileNameProperty : Migration
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

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfile_AspNetUsers_UserId",
                schema: "public",
                table: "UserProfile");

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

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProfile",
                schema: "public",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "Birthdate",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Fax",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "OtherPhone",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Pronouns",
                schema: "public",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "ProposalId",
                schema: "public",
                table: "Documents",
                newName: "PartnerId");

            migrationBuilder.RenameColumn(
                name: "FundingOpportunityId",
                schema: "public",
                table: "Documents",
                newName: "InteractionId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_ProposalId",
                schema: "public",
                table: "Documents",
                newName: "IX_Documents_PartnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_FundingOpportunityId",
                schema: "public",
                table: "Documents",
                newName: "IX_Documents_InteractionId");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "public",
                table: "UserProfile",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "public",
                table: "UserProfile",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "public",
                table: "UserProfile",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "DutyStation",
                schema: "public",
                table: "UserProfile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrgUnit",
                schema: "public",
                table: "UserProfile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                schema: "public",
                table: "UserProfile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                schema: "public",
                table: "UserProfile",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                schema: "public",
                table: "UserProfile",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                schema: "public",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentTypeId",
                schema: "public",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Suffix",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Salutation",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MiddleName",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MailingStreet2",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MailingStreet",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MailingStateProvince",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MailingPostalCode",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MailingCountry",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MailingCity",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Department",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AssistantPhone",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AssistantEmail",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Assistant",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                schema: "public",
                table: "Contacts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProfile",
                schema: "public",
                table: "UserProfile",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "AiChatSession",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    AiGenerateTitle = table.Column<bool>(type: "boolean", nullable: false),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    Starred = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiChatSession", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiPrompt",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    PromptFunction = table.Column<string>(type: "text", nullable: false),
                    Prompt = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GenerationConfig = table.Column<string>(type: "text", nullable: false),
                    ContentConfig = table.Column<string>(type: "text", nullable: false),
                    ToolsConfig = table.Column<string>(type: "text", nullable: true),
                    SafetySettings = table.Column<string>(type: "text", nullable: true),
                    Project = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    AdminCanChange = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiPrompt", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityEmbeddings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    EntityData = table.Column<string>(type: "text", nullable: false),
                    FullEmbedding = table.Column<byte[]>(type: "vector(768)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityEmbeddings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Interactions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EmailAddresses = table.Column<string>(type: "text", nullable: true),
                    PhoneNumbers = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    GmailThreadId = table.Column<string>(type: "text", nullable: true),
                    GmailMessageId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ContactId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_Interactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interactions_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalSchema: "public",
                        principalTable: "Contacts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LiaisonOffices",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_LiaisonOffices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Links",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Entity = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
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
                    table.PrimaryKey("PK_Links", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    ResponseType = table.Column<string>(type: "text", nullable: false),
                    RecordData = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationHierarchies",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    IsSelfManagementEnabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_OrganizationHierarchies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationHierarchies_OrganizationHierarchies_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "public",
                        principalTable: "OrganizationHierarchies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartnerTrees",
                schema: "public",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Parent = table.Column<string>(type: "text", nullable: true),
                    PartnerCategoryCode = table.Column<string>(type: "text", nullable: true),
                    PartnerGroupCode = table.Column<string>(type: "text", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_PartnerTrees", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "SavedFilters",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SearchCriteria = table.Column<string>(type: "text", nullable: false),
                    SearchText = table.Column<string>(type: "text", nullable: true),
                    IsAdvancedSearch = table.Column<bool>(type: "boolean", nullable: false),
                    OrderByField = table.Column<string>(type: "text", nullable: true),
                    Ascending = table.Column<bool>(type: "boolean", nullable: true),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    LastUsedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedFilters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    GlobalFilterJson = table.Column<string>(type: "text", nullable: true),
                    AdditionalSettingsJson = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_UserProfile_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InteractionContacts",
                schema: "public",
                columns: table => new
                {
                    InteractionId = table.Column<int>(type: "integer", nullable: false),
                    ContactId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionContacts", x => new { x.InteractionId, x.ContactId });
                    table.ForeignKey(
                        name: "FK_InteractionContacts_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalSchema: "public",
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InteractionContacts_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalSchema: "public",
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InteractionUsers",
                schema: "public",
                columns: table => new
                {
                    InteractionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionUsers", x => new { x.InteractionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_InteractionUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InteractionUsers_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalSchema: "public",
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnitRelationships",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationHierarchyId = table.Column<int>(type: "integer", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_OrganizationUnitRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitRelationships_OrganizationHierarchies_Organ~",
                        column: x => x.OrganizationHierarchyId,
                        principalSchema: "public",
                        principalTable: "OrganizationHierarchies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Partners",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    PartnerGroupCode = table.Column<string>(type: "text", nullable: true),
                    UniqueKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerCategoryInternalKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerCategoryKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerTypeKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerShortDescription = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PartnerLongDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PartnerCategoryId = table.Column<int>(type: "integer", nullable: true),
                    ErpDimValue = table.Column<int>(type: "integer", nullable: true),
                    LiaisonOfficeId = table.Column<int>(type: "integer", nullable: true),
                    PartnerFocalPointUserId = table.Column<int>(type: "integer", nullable: true),
                    UNAndStateEntity = table.Column<bool>(type: "boolean", nullable: false),
                    KeyGlobalPartner = table.Column<bool>(type: "boolean", nullable: false),
                    UNSecretariatPartner = table.Column<bool>(type: "boolean", nullable: false),
                    DueDiligenceRequired = table.Column<int>(type: "integer", nullable: true),
                    DueDiligenceApproval = table.Column<int>(type: "integer", nullable: true),
                    DueDiligenceApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDiligenceExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PartnerApprovalStatus = table.Column<int>(type: "integer", nullable: false),
                    PartnerApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PartnerApprovalReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PartnerApprovedBy = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PartnerLevyStatus = table.Column<int>(type: "integer", nullable: true),
                    ReasonForLevy = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LevyTreatment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PooledFund = table.Column<bool>(type: "boolean", nullable: false),
                    CanCreateNewOpportunities = table.Column<bool>(type: "boolean", nullable: false),
                    ReasonForNoNewOpportunity = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Partners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partners_LiaisonOffices_LiaisonOfficeId",
                        column: x => x.LiaisonOfficeId,
                        principalSchema: "public",
                        principalTable: "LiaisonOffices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Partners_PartnerTrees_PartnerGroupCode",
                        column: x => x.PartnerGroupCode,
                        principalSchema: "public",
                        principalTable: "PartnerTrees",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "Engagements",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaseEngagement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EngagementDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EngagementLongDescription = table.Column<string>(type: "text", nullable: true),
                    EngagementStageDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EngagementImplementationStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EngagementImplementationEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PartnerId = table.Column<int>(type: "integer", nullable: true),
                    ImplementationCountriesDescriptionConcatenated = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_Engagements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Engagements_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "public",
                        principalTable: "Partners",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InteractionPartners",
                schema: "public",
                columns: table => new
                {
                    InteractionId = table.Column<int>(type: "integer", nullable: false),
                    PartnerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionPartners", x => new { x.InteractionId, x.PartnerId });
                    table.ForeignKey(
                        name: "FK_InteractionPartners_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalSchema: "public",
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InteractionPartners_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "public",
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ContactId",
                schema: "public",
                table: "Documents",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentTypeId",
                schema: "public",
                table: "Documents",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_PartnerId",
                schema: "public",
                table: "Contacts",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Engagements_PartnerId",
                schema: "public",
                table: "Engagements",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityEmbeddings_EntityId",
                schema: "public",
                table: "EntityEmbeddings",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityEmbeddings_EntityName",
                schema: "public",
                table: "EntityEmbeddings",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_EntityEmbeddings_EntityName_EntityId",
                schema: "public",
                table: "EntityEmbeddings",
                columns: new[] { "EntityName", "EntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InteractionContacts_ContactId",
                schema: "public",
                table: "InteractionContacts",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionPartners_PartnerId",
                schema: "public",
                table: "InteractionPartners",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_ContactId",
                schema: "public",
                table: "Interactions",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionUsers_UserId",
                schema: "public",
                table: "InteractionUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationHierarchies_ParentId",
                schema: "public",
                table: "OrganizationHierarchies",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRelationships_EntityId_EntityType",
                schema: "public",
                table: "OrganizationUnitRelationships",
                columns: new[] { "EntityId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRelationships_EntityId_EntityType_Organizat~",
                schema: "public",
                table: "OrganizationUnitRelationships",
                columns: new[] { "EntityId", "EntityType", "OrganizationHierarchyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRelationships_OrganizationHierarchyId",
                schema: "public",
                table: "OrganizationUnitRelationships",
                column: "OrganizationHierarchyId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_LiaisonOfficeId",
                schema: "public",
                table: "Partners",
                column: "LiaisonOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerGroupCode",
                schema: "public",
                table: "Partners",
                column: "PartnerGroupCode");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                schema: "public",
                table: "UserPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Partners_PartnerId",
                schema: "public",
                table: "Contacts",
                column: "PartnerId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Contacts_ContactId",
                schema: "public",
                table: "Documents",
                column: "ContactId",
                principalSchema: "public",
                principalTable: "Contacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_DocumentTypes_DocumentTypeId",
                schema: "public",
                table: "Documents",
                column: "DocumentTypeId",
                principalSchema: "public",
                principalTable: "DocumentTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Interactions_InteractionId",
                schema: "public",
                table: "Documents",
                column: "InteractionId",
                principalSchema: "public",
                principalTable: "Interactions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Partners_PartnerId",
                schema: "public",
                table: "Documents",
                column: "PartnerId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfile_AspNetUsers_UserId",
                schema: "public",
                table: "UserProfile",
                column: "UserId",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Partners_PartnerId",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Contacts_ContactId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_DocumentTypes_DocumentTypeId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Interactions_InteractionId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Partners_PartnerId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfile_AspNetUsers_UserId",
                schema: "public",
                table: "UserProfile");

            migrationBuilder.DropTable(
                name: "AiChatSession",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AiPrompt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DocumentTypes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Engagements",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EntityEmbeddings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InteractionContacts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InteractionPartners",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InteractionUsers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Links",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OrganizationUnitRelationships",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SavedFilters",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UserPreferences",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Partners",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Interactions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OrganizationHierarchies",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LiaisonOffices",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PartnerTrees",
                schema: "public");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProfile",
                schema: "public",
                table: "UserProfile");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ContactId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DocumentTypeId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_PartnerId",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "DutyStation",
                schema: "public",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "OrgUnit",
                schema: "public",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "Position",
                schema: "public",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                schema: "public",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                schema: "public",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "ContactId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DocumentTypeId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                schema: "public",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                schema: "public",
                table: "Documents",
                newName: "ProposalId");

            migrationBuilder.RenameColumn(
                name: "InteractionId",
                schema: "public",
                table: "Documents",
                newName: "FundingOpportunityId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_PartnerId",
                schema: "public",
                table: "Documents",
                newName: "IX_Documents_ProposalId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_InteractionId",
                schema: "public",
                table: "Documents",
                newName: "IX_Documents_FundingOpportunityId");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "public",
                table: "UserProfile",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "public",
                table: "UserProfile",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "public",
                table: "UserProfile",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "UserProfile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Suffix",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Salutation",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MiddleName",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MailingStreet2",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MailingStreet",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MailingStateProvince",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MailingPostalCode",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MailingCountry",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MailingCity",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Department",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssistantPhone",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssistantEmail",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Assistant",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Birthdate",
                schema: "public",
                table: "Contacts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OtherPhone",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Pronouns",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProfile",
                schema: "public",
                table: "UserProfile",
                column: "Id");

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
                    SubmissionDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "IX_FundingOpportunities_CurrencyId",
                schema: "public",
                table: "FundingOpportunities",
                column: "CurrencyId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfile_AspNetUsers_UserId",
                schema: "public",
                table: "UserProfile",
                column: "UserId",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
