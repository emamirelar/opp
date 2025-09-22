using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities.Common;
using Microsoft.Extensions.Hosting;
using UNOPS.PAO.Domain.Entities;
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
    public DbSet<LiaisonOffice> LiaisonOffices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure AiPrompt table mapping
        modelBuilder.Entity<AiPrompt>()
            .ToTable("AiPrompt") // Map to table "AiPrompt" without "s"
            .HasIndex(p => p.Type)
            .IsUnique(); // Ensure Type is unique

        modelBuilder
            .Entity<UNOPSContact>();
        //.HasPrincipalKey(x => x.ContactNumber);

        modelBuilder
            .Entity<UNOPSPartner>()
            .HasMany(x => x.Projects)
            .WithMany(x => x.Partners)
            .UsingEntity("PartnerProjects");
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
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<WorkPackage> WorkPackages { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<BudgetLine> BudgetLines { get; set; }
    public DbSet<Donor> Donors { get; set; }
    public new DbSet<UNOPSContact> Contacts { get; set; }
    public new DbSet<UNOPSInteraction> Interactions { get; set; }

    public new DbSet<UNOPSPartner> Partners { get; set; }
    public new DbSet<UNOPSLink> Links { get; set; }

    public new DbSet<AiChatSession> AiChatSession { get; set; }
    public new DbSet<UNOPSDocument> Documents { get; set; }
    public DbSet<OrganizationHierarchy> OrganizationHierarchies { get; set; }
    public new DbSet<UNOPSPartnerTree> PartnerTrees { get; set; }

    // Add DbSet for OrganizationUnitRelationship
    public DbSet<OrganizationUnitRelationship> OrganizationUnitRelationships { get; set; }

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
    
    // Base Engagement entities (externally managed, read-only)
    public DbSet<BaseEngagement> BaseEngagements { get; set; }
    public DbSet<BaseEngagementPartners> BaseEngagementPartners { get; set; }
}