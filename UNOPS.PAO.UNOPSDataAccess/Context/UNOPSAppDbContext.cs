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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        // Configure Partner to PartnerTree relationship properly with only one foreign key
        modelBuilder
            .Entity<Partner>()
            .HasOne(x => x.PartnerGroup)
            .WithMany(x => x.Partners)
            .HasForeignKey(x => x.PartnerGroupCode)
            .HasPrincipalKey(x => x.Code);

        // Ignore any convention-based relationship that would create a PartnerTreeCode column
        modelBuilder
            .Entity<Partner>()
            .Ignore("PartnerTree");
        
        modelBuilder
            .Entity<UNOPSLink>();

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
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<WorkPackage> WorkPackages { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<BudgetLine> BudgetLines { get; set; }
    public DbSet<Donor> Donors { get; set; }
    public new DbSet<UNOPSContact> Contacts { get; set; }
    public new DbSet<UNOPSInteraction> Interactions { get; set; }
    public new DbSet<UNOPSPartnerTree> PartnerTrees { get; set; }
    public new DbSet<UNOPSPartner> Partners { get; set; }
    public new DbSet<UNOPSLink> Links { get; set; }

    public new DbSet<AiChatSession> AiChatSession { get; set; }
    public new DbSet<UNOPSDocument> Documents { get; set; }
    public DbSet<OrganizationHierarchy> OrganizationHierarchies { get; set; }

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
}