using UNOPS.PAO.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.UNOPSDomain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace UNOPS.PAO.DataAccess.Context;

public class AppDbContext : AuditableDbContext<int, int>
{
    private readonly UserResolverService<int> _userResolverService;
    public AppDbContext(DbContextOptions<AppDbContext> options, UserResolverService<int> userResolverService, IDbContextSchema schema)
        : base(options, userResolverService, schema)
    {
    }

    protected AppDbContext(DbContextOptions options, UserResolverService<int> userResolverService, IDbContextSchema schema) : base(options, userResolverService, schema)
    {
    }

    public DbSet<PAOUser> PAOUsers { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<Country> Countries { get; set; }

    public DbSet<EligibleEntity> EligibleEntities { get; set; }

    public DbSet<WorkflowLog> WorkflowLogs { get; set; }
    public DbSet<EntityUserRole> EntityUserRoles { get; set; }

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Interaction> Interactions { get; set; }
    public DbSet<Partner> Partners { get; set; }
    public DbSet<PartnerTree> PartnerTrees { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentRelationship> DocumentRelationships { get; set; }
    public DbSet<OrganizationUnitRelationship> OrganizationUnitRelationships { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<UNOPS.PAO.Domain.Entities.Link> Links { get; set; }
    public DbSet<OrganizationHierarchy> OrganizationHierarchies { get; set; }

    public DbSet<EntityEmbeddings> EntityEmbeddings { get; set; }
    public DbSet<InteractionContact> InteractionContacts { get; set; }
    public DbSet<InteractionUser> InteractionUsers { get; set; }
    public DbSet<InteractionPartner> InteractionPartners { get; set; }
    public DbSet<UserInfo> UserInfos { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }

    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SavedFilter> SavedFilters { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings
            .Ignore(RelationalEventId.PendingModelChangesWarning));
    }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");

        modelBuilder
            .Entity<IdentityUserRole<int>>()
            .ToTable("AspNetUserRoles", t => t.ExcludeFromMigrations())
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // Configure PAOUser and UserProfile relationship
        modelBuilder
            .Entity<PAOUser>()
            .ToTable("AspNetUsers", t => t.ExcludeFromMigrations())
            .HasOne(x => x.UserProfile)
            .WithOne()
            .HasForeignKey<UserProfile>(x => x.UserId)
            .IsRequired(false); // Make it optional to avoid constraint issues during creation

        modelBuilder
            .Entity<UserProfile>()
            .Property(up => up.UserId)
            .IsRequired();

        modelBuilder
            .Entity<UserProfile>()
            .HasIndex(up => up.UserId)
            .IsUnique();

        modelBuilder
            .Entity<Partner>(p =>
            {
                // Configure one-to-many relationship with Contacts
                p.HasMany(x => x.Contacts)
                    .WithOne(c => c.Partner)
                    .HasForeignKey(c => c.PartnerId)
                    .OnDelete(DeleteBehavior.Cascade);

                p.Ignore(x => x.OrganizationUnitRelationships);
            });

        modelBuilder
            .Entity<Contact>();
        
        modelBuilder.Entity<EntityUserRole>(entity =>
        {
            entity.HasOne(e => e.UserRole)
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.RoleId });
        });

        modelBuilder.Entity<Interaction>(entity =>
        {
            entity.Property(e => e.EmailAddresses)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                            new ValueComparer<List<string>>(
                                (c1, c2) => c1.SequenceEqual(c2),
                                  c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                                  c => c.ToList()))
                  .HasColumnType("text");

        entity.Property(e => e.PhoneNumbers)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                            new ValueComparer<List<string>>(
                              (c1, c2) => c1.SequenceEqual(c2),
                              c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                              c => c.ToList()))
                  .HasColumnType("text");

            entity.HasMany(i => i.InteractionContacts)
                .WithOne(ic => ic.Interaction)
                .HasForeignKey(ic => ic.InteractionId);

            entity.HasMany(i => i.InteractionUsers)
                .WithOne(iu => iu.Interaction)
                .HasForeignKey(iu => iu.InteractionId);

            entity.HasMany(i => i.InteractionPartners)
                .WithOne(ip => ip.Interaction)
                .HasForeignKey(ip => ip.InteractionId);
        });

        modelBuilder
            .Entity<InteractionContact>()
            .HasKey(ic => new { ic.InteractionId, ic.ContactId });

        modelBuilder
            .Entity<InteractionPartner>()
            .HasKey(ip => new { ip.InteractionId, ip.PartnerId });

        modelBuilder
            .Entity<InteractionUser>()
            .HasKey(iu => new { iu.InteractionId, iu.UserId });

        modelBuilder
            .Entity<PartnerTree>();

        // Add discriminator configuration for PartnerTree inheritance hierarchy
        modelBuilder
            .Entity<PartnerTree>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<PartnerTree>("PartnerTree");

        modelBuilder
            .Entity<AiPrompt>();

        modelBuilder
            .Entity<AiChatSession>();

        modelBuilder.Entity<Document>(doc =>
        {
            doc.HasOne(x => x.DocumentType)
                .WithMany()
                .HasForeignKey(x => x.DocumentTypeId);
        });

        modelBuilder.Entity<DocumentRelationship>()
            .HasKey(dr => new { dr.DocumentId, dr.EntityId, dr.EntityType });

        modelBuilder.Entity<DocumentRelationship>(entity =>
        {
            entity.HasKey(e => new { e.DocumentId, e.EntityId, e.EntityType });

            entity.HasOne(e => e.Document)
                .WithMany(d => d.DocumentRelationships)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.EntityId)
                .IsRequired();

            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => new { e.EntityId, e.EntityType });
        });

        modelBuilder.Entity<OrganizationUnitRelationship>(entity =>
        {
            
            entity.HasOne(e => e.OrganizationHierarchy)
                .WithMany(o => o.EntityRelationships)
                .HasForeignKey(e => e.OrganizationHierarchyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.EntityId)
                .IsRequired();

            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => new { e.EntityId, e.EntityType });
            
            // Add unique constraint for the business logic (one relationship per entity/org unit combo)
            entity.HasIndex(e => new { e.EntityId, e.EntityType, e.OrganizationHierarchyId })
                .IsUnique();

        });

        modelBuilder
            .Entity<UNOPS.PAO.Domain.Entities.Link>()
            .ToTable("Links", "public")
            .HasDiscriminator<string>("Discriminator")
            .HasValue<UNOPS.PAO.Domain.Entities.Link>("Link")
            .HasValue<UNOPSLink>("UNOPSLink");
            
        modelBuilder
            .Entity<UNOPSLink>();

        modelBuilder.Entity<EntityEmbeddings>(entity =>
        {
            entity.HasIndex(e => e.EntityName);
            entity.HasIndex(e => e.EntityId);
            entity.Property(e => e.FullEmbedding)
              .HasColumnType("vector(768)");
            entity.HasIndex(e => new { e.EntityName, e.EntityId })
                    .IsUnique(); // This ensures uniqueness at the database level
        });

        modelBuilder.Entity<OrganizationHierarchy>(entity =>
        {
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Type)
                .HasColumnType("text")
                .IsRequired();

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.ToTable("UserInfos", "public");
            entity.HasKey(e => e.UserId);
            
            entity.Property(e => e.Name)
                .HasMaxLength(200);
            
            entity.Property(e => e.UserEmail)
                .HasMaxLength(256);
            
            entity.Property(e => e.OrgUnit)
                .HasMaxLength(200);
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.ToTable("UserPreferences", "public");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityByDefaultColumn();
            
            entity.Property(e => e.UserId)
                .IsRequired();
            
            // Foreign key relationship to UserProfile
            entity.HasOne(e => e.UserProfile)
                .WithOne(up => up.UserPreference)
                .HasForeignKey<UserPreference>(e => e.UserId)
                .HasPrincipalKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.GlobalFilterJson)
                .HasColumnType("text");
            
            entity.Property(e => e.AdditionalSettingsJson)
                .HasColumnType("text");
        });

        // Ignore GlobalFilters class - it's not an entity, just a plain class for JSON serialization
        modelBuilder.Ignore<GlobalFilters>();
    }
}
