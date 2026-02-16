using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;
using Microsoft.Extensions.Hosting;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Authorization;

namespace UNOPS.PAO.UNOPSDataAccess.Context;

public class UNOPSAppDbContext : AppDbContext
{
    public UNOPSAppDbContext(DbContextOptions<UNOPSAppDbContext> options, UserResolverService<int> userService, IDbContextSchema schema)
        : base(options, userService, schema)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings
            .Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    // Entity permission for RBAC
    public DbSet<EntityPermission> EntityPermissions { get; set; }
    
    // Reference data tables
    public new DbSet<LiaisonOffice> LiaisonOffices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TEMPORARY: Configure EntityEmbeddings to use bytea instead of vector until pgvector is available
        modelBuilder.Entity<EntityEmbeddings>(entity =>
        {
            entity.Property(e => e.FullEmbedding)
                  .HasColumnType("bytea");  // Use bytea instead of vector(768)
        });

        // Configure AiPrompt table mapping
        modelBuilder.Entity<AiPrompt>()
            .ToTable("AiPrompt") // Map to table "AiPrompt" without "s"
            .HasIndex(p => p.Type)
            .IsUnique(); // Ensure Type is unique

        modelBuilder
            .Entity<UNOPSContact>();
        //.HasPrincipalKey(x => x.ContactNumber);

        modelBuilder
            .Entity<UNOPSPartner>();
        //need to make PartnerCode unique but can not autogenerate as this can conflict with existing data from ERP
        //making PartnerCode optional for now
        //.HasIndex(x => x.PartnerCode) 
        //.IsUnique();
        
        // // Ignore any convention-based relationship that would create a PartnerTreeCode column
        // modelBuilder
        //     .Entity<Partner>()
        //     .Ignore("PartnerTree");

        // Configure Partner to LiaisonOffice relationship
        modelBuilder
            .Entity<Partner>()
            .HasOne(p => p.LiaisonOffice)
            .WithMany(lo => lo.Partners)
            .HasForeignKey(p => p.LiaisonOfficeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure unique constraint for ErpDimValue (allows null but enforces uniqueness when not null)
        modelBuilder
            .Entity<Partner>()
            .HasIndex(p => p.ErpDimValue)
            .IsUnique()
            .HasFilter("\"ErpDimValue\" IS NOT NULL");

        // Configure Partner to PartnerTree (PartnerGroup) relationship
        modelBuilder
            .Entity<Partner>()
            .HasOne(p => p.PartnerGroup)
            .WithMany(pt => pt.Partners)
            .HasForeignKey(p => p.PartnerGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure LiaisonOffice entity
        modelBuilder
            .Entity<LiaisonOffice>()
            .HasIndex(lo => lo.Code)
            .IsUnique();

        // Configure unique constraint for PartnerTree Code
        modelBuilder
            .Entity<PartnerTree>()
            .HasIndex(pt => pt.Code)
            .IsUnique()
            .HasFilter("\"Code\" IS NOT NULL");


        
        modelBuilder
            .Entity<UNOPSLink>();

        // // Configure PartnerTree Id to be auto-generated
        // modelBuilder
        //     .Entity<PartnerTree>()
        //     .Property(p => p.Id)
        //     .ValueGeneratedOnAdd();

        // Complete discriminator configuration for PartnerTree inheritance hierarchy
        modelBuilder
            .Entity<UNOPSPartnerTree>()
            .HasDiscriminator().HasValue("UNOPSPartnerTree");

        modelBuilder
            .Entity<OrganizationHierarchy>()
            .HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Entities table
        modelBuilder
            .Entity<Entities>()
            .HasIndex(e => e.EntityName)
            .IsUnique();

        // Configure EntityManager
        modelBuilder
            .Entity<EntityManager>()
            .HasIndex(e => e.EntityName)
            .IsUnique();

        // Configure EntityFieldManager relationship
        modelBuilder
            .Entity<EntityFieldManager>()
            .HasOne(e => e.EntityManager)
            .WithMany(e => e.EntityFields)
            .HasForeignKey(e => e.EntityManagerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure OrganizationUnitRelationship entity
        modelBuilder
            .Entity<OrganizationUnitRelationship>()
            .HasOne(r => r.OrganizationHierarchy)
            .WithMany(o => o.EntityRelationships)
            .HasForeignKey(r => r.OrganizationHierarchyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create composite index for OrganizationUnitRelationship for better query performance
        modelBuilder
            .Entity<OrganizationUnitRelationship>()
            .HasIndex(r => new { r.EntityId, r.EntityType, r.OrganizationHierarchyId })
            .IsUnique(); // Prevent duplicate relationships

        // Create index for querying by EntityId and EntityType
        modelBuilder
            .Entity<OrganizationUnitRelationship>()
            .HasIndex(r => new { r.EntityId, r.EntityType });

        // BaseEngagement configuration (externally managed READ-ONLY table)
        modelBuilder.Entity<BaseEngagement>(entity =>
        {
            entity.ToTable("BaseEngagements"); // Maps to table created by External Data Service
            entity.HasKey(e => e.Id);
            
            // Map to the actual column name from external service
            entity.Property(e => e.EngagementNumber)
                  .HasColumnName("BaseEngagement") // Map to actual column name
                  .IsRequired()
                  .HasMaxLength(50);
                  
            entity.Property(e => e.EngagementStage)
                  .HasMaxLength(100);
                  
            entity.Property(e => e.EngagementStageDescription)
                  .HasMaxLength(500);
                  
            entity.Property(e => e.BusinessDeveloper)
                  .HasMaxLength(255);
                  
            entity.Property(e => e.BusinessDeveloperName)
                  .HasMaxLength(255);
                  
            entity.Property(e => e.BusinessDeveloperEmailAddress)
                  .HasMaxLength(255);
                  
            entity.Property(e => e.EngagementProjectExecutive)
                  .HasMaxLength(255);
                  
            entity.Property(e => e.EngagementProjectExecutiveName)
                  .HasMaxLength(255);
            
            entity.Property(e => e.EngagementAmount)
                  .HasColumnType("decimal(18,2)");
            
            // Audit field from External Data Service
            entity.Property(e => e.IsDeleted)
                  .HasDefaultValue(false);
            
            // Text fields
            entity.Property(e => e.ImplementationCountriesList)
                  .HasColumnType("text");
                  
            entity.Property(e => e.OutputsList)
                  .HasColumnType("text");
                  
            entity.Property(e => e.SDGList)
                  .HasColumnType("text");
                  
            entity.Property(e => e.EngagementDescription)
                  .HasColumnType("text");
                  
            entity.Property(e => e.EngagementLongDescription)
                  .HasColumnType("text");
        });
        
        // BaseEngagementPartners configuration (externally managed READ-ONLY table)
        modelBuilder.Entity<BaseEngagementPartners>(entity =>
        {
            entity.ToTable("BaseEngagementPartners"); // Maps to table created by External Data Service
            entity.HasKey(e => e.Id);
            
            // Property configurations (must match external data service field mappings)
            entity.Property(e => e.Key)
                  .IsRequired()
                  .HasMaxLength(200);
                  
            entity.Property(e => e.EngagementNumber)
                  .HasColumnName("BaseEngagement") // Map to actual column name from external service
                  .IsRequired()
                  .HasMaxLength(50);
                  
            entity.Property(e => e.PartnerType)
                  .HasMaxLength(50);
                  
            entity.Property(e => e.Partner)
                  .HasMaxLength(50);
                  
            entity.Property(e => e.PartnerDescription)
                  .HasMaxLength(255);
            
            // Audit field from External Data Service
            entity.Property(e => e.IsDeleted)
                  .HasDefaultValue(false);
            
            // IMPORTANT: NO foreign key constraints - soft relationships only
            // Navigation properties are configured for LINQ joins but create no DB constraints
            entity.HasOne(e => e.BaseEngagementEntity)
                  .WithMany(e => e.EngagementPartners)
                  .HasForeignKey(e => e.BaseEngagementId)
                  .OnDelete(DeleteBehavior.NoAction) // No cascade, no constraints
                  .HasConstraintName(null); // Explicitly remove FK constraint
                  
            entity.HasOne(e => e.PartnerEntity)
                  .WithMany()
                  .HasForeignKey(e => e.PartnerId)
                  .OnDelete(DeleteBehavior.NoAction) // No cascade, no constraints
                  .HasConstraintName(null); // Explicitly remove FK constraint
        });

        // Configure Risk entity
        modelBuilder.Entity<Risk>(entity =>
        {
            entity.ToTable("Risks");

            entity.HasIndex(e => new { e.EntityType, e.EntityId })
                  .HasDatabaseName("IX_Risks_EntityType_EntityId");

            entity.Property(e => e.EntityType)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.Description);

            entity.Property(e => e.Recommendation);

            // Legacy enum fields (kept for backward compatibility)
            entity.Property(e => e.Impact)
                  .HasConversion<int>();

            entity.Property(e => e.RiskStatus)
                  .IsRequired()
                  .HasConversion<int>()
                  .HasDefaultValue(RiskStatus.Open);

            // FK to RiskType (mandatory)
            entity.HasOne(e => e.RiskTypeEntity)
                  .WithMany()
                  .HasForeignKey(e => e.RiskTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // FK to RiskCategory (mandatory - Level 3 leaf)
            entity.HasOne(e => e.RiskCategory)
                  .WithMany()
                  .HasForeignKey(e => e.RiskCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // FK to RiskProbability (mandatory)
            entity.HasOne(e => e.RiskProbabilityEntity)
                  .WithMany()
                  .HasForeignKey(e => e.RiskProbabilityId)
                  .OnDelete(DeleteBehavior.Restrict);

            // FK to RiskProximity (mandatory)
            entity.HasOne(e => e.RiskProximityEntity)
                  .WithMany()
                  .HasForeignKey(e => e.RiskProximityId)
                  .OnDelete(DeleteBehavior.Restrict);

            // FK to RiskImpactLevel (mandatory)
            entity.HasOne(e => e.RiskImpactLevelEntity)
                  .WithMany()
                  .HasForeignKey(e => e.RiskImpactLevelId)
                  .OnDelete(DeleteBehavior.Restrict);

            // FK to RiskResponseType (optional - mandatory only for Opportunity type)
            entity.HasOne(e => e.RiskResponseTypeEntity)
                  .WithMany()
                  .HasForeignKey(e => e.RiskResponseTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // FK to PreDefinedHighRisk (optional - when created from checklist)
            entity.HasOne(e => e.PreDefinedHighRisk)
                  .WithMany()
                  .HasForeignKey(e => e.PreDefinedHighRiskId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        #region Risk Lookup Tables Configuration

        // Configure RiskType entity
        modelBuilder.Entity<RiskType>(entity =>
        {
            entity.ToTable("RiskTypes");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
        });

        // Configure RiskProbability entity
        modelBuilder.Entity<RiskProbability>(entity =>
        {
            entity.ToTable("RiskProbabilities");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
        });

        // Configure RiskProximity entity
        modelBuilder.Entity<RiskProximity>(entity =>
        {
            entity.ToTable("RiskProximities");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(30);
        });

        // Configure RiskImpactLevel entity
        modelBuilder.Entity<RiskImpactLevel>(entity =>
        {
            entity.ToTable("RiskImpactLevels");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
        });

        // Configure RiskResponseType entity
        modelBuilder.Entity<RiskResponseType>(entity =>
        {
            entity.ToTable("RiskResponseTypes");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
        });

        #endregion

        #region Risk Category and PreDefined High Risk Configuration

        // Configure RiskCategory entity (3-level hierarchy)
        modelBuilder.Entity<RiskCategory>(entity =>
        {
            entity.ToTable("RiskCategories");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.ShortCode);
            entity.HasIndex(e => e.Level);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ShortCode).IsRequired().HasMaxLength(50);

            // Self-referential FK for hierarchy
            entity.HasOne(e => e.ParentCategory)
                  .WithMany(e => e.ChildCategories)
                  .HasForeignKey(e => e.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure PreDefinedHighRisk entity
        modelBuilder.Entity<PreDefinedHighRisk>(entity =>
        {
            entity.ToTable("PreDefinedHighRisks");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.OupQuestionId);
            entity.HasIndex(e => e.IsAutoDetectable);
            entity.HasIndex(e => e.CategoryCode);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DisplayCode).HasMaxLength(20);
            entity.Property(e => e.ShortTitle).HasMaxLength(255);
            entity.Property(e => e.CategoryCode).HasMaxLength(50);
            entity.Property(e => e.Level2Code).HasMaxLength(10);
            entity.Property(e => e.DetectionRuleType).HasMaxLength(50);

            // FK to RiskCategory (Level 3)
            entity.HasOne(e => e.RiskCategory)
                  .WithMany()
                  .HasForeignKey(e => e.RiskCategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        #endregion

        #region Opportunity Configuration

        // Configure Opportunity → Executive relationship
        modelBuilder.Entity<Opportunity>(entity =>
        {
            // Executive assignment (set during Go decision)
            entity.HasOne(e => e.Executive)
                  .WithMany()
                  .HasForeignKey(e => e.ExecutiveId)
                  .OnDelete(DeleteBehavior.SetNull); // Executive deletion shouldn't delete Opportunity
        });

        #endregion

        #region Collaborator Expertise Configuration

        // Configure CollaboratorExpertise entity (lookup table)
        modelBuilder.Entity<CollaboratorExpertise>(entity =>
        {
            entity.ToTable("CollaboratorExpertises");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure OpportunityCollaboratorExpertise junction table
        modelBuilder.Entity<OpportunityCollaboratorExpertise>(entity =>
        {
            entity.ToTable("OpportunityCollaboratorExpertises");
            
            // Composite unique index to prevent duplicate expertise assignments
            entity.HasIndex(e => new { e.OpportunityCollaboratorId, e.CollaboratorExpertiseId })
                  .IsUnique();

            // FK to OpportunityCollaborator
            entity.HasOne(e => e.OpportunityCollaborator)
                  .WithMany(c => c.Expertises)
                  .HasForeignKey(e => e.OpportunityCollaboratorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // FK to CollaboratorExpertise
            entity.HasOne(e => e.CollaboratorExpertise)
                  .WithMany(e => e.CollaboratorExpertises)
                  .HasForeignKey(e => e.CollaboratorExpertiseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        #endregion
    }

    public new DbSet<UNOPSContact> Contacts { get; set; }
    public new DbSet<UNOPSInteraction> Interactions { get; set; }

    public new DbSet<UNOPSPartner> Partners { get; set; }
    public new DbSet<UNOPSLink> Links { get; set; }

    public DbSet<AiChatSession> AiChatSession { get; set; }
    public new DbSet<UNOPSDocument> Documents { get; set; }
    //Removed OrganizationHierarchies and OrganizationUnitRelationships from UNOPSAppDbContext to avoid shadowing issue as they are already present in AppDbContext
    public new DbSet<UNOPSPartnerTree> PartnerTrees { get; set; }
    public new DbSet<EntityEmbeddings> EntityEmbeddings { get; set; }
    public new DbSet<InteractionContact> InteractionContacts { get; set; }
    public new DbSet<InteractionUser> InteractionUsers { get; set; }
    public new DbSet<InteractionPartner> InteractionPartners { get; set; }
    
    // Entity reference table
    public DbSet<Entities> Entities { get; set; }
    
    // New entity configuration DbSets
    public DbSet<EntityManager> EntityManagers { get; set; }
    public DbSet<EntityFieldManager> EntityFieldManagers { get; set; }
    
    // Email notification tracking (generalized for all email notifications)
    public DbSet<EmailNotificationLog> EmailNotificationLogs { get; set; }
    
    // AI-related DbSets
    public DbSet<AiPrompt> AiPrompts { get; set; }
    
    // Seed script tracking
    public DbSet<SeedScript> SeedScripts { get; set; }

    // Risk register
    public DbSet<Risk> Risks { get; set; }
    
    // Risk lookup tables (oUP aligned)
    public DbSet<RiskType> RiskTypes { get; set; }
    public DbSet<RiskProbability> RiskProbabilities { get; set; }
    public DbSet<RiskProximity> RiskProximities { get; set; }
    public DbSet<RiskImpactLevel> RiskImpactLevels { get; set; }
    public DbSet<RiskResponseType> RiskResponseTypes { get; set; }
    
    // Risk categories (3-level hierarchy)
    public DbSet<RiskCategory> RiskCategories { get; set; }
    
    // PreDefined High Risks (EAC checklist items)
    public DbSet<PreDefinedHighRisk> PreDefinedHighRisks { get; set; }
    
    // Base Engagement entities (externally managed, read-only)
    public DbSet<BaseEngagement> BaseEngagements { get; set; }
    public DbSet<BaseEngagementPartners> BaseEngagementPartners { get; set; }
    
    // Collaborator Expertise lookup and junction table
    public DbSet<CollaboratorExpertise> CollaboratorExpertises { get; set; }
    public DbSet<OpportunityCollaboratorExpertise> OpportunityCollaboratorExpertises { get; set; }
}