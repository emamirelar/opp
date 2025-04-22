using UNOPS.PAO.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.UNOPSDomain.Entities;

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

    public DbSet<GrantUser> GrantUsers { get; set; }
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
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<UNOPS.PAO.Domain.Entities.Link> Links { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<PartnerCategory> PartnerCategories { get; set; }

    public DbSet<EntityEmbeddings> EntityEmbeddings { get; set; }

    public DbSet<Notification> Notifications { get; set; }

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

        modelBuilder
            .Entity<GrantUser>()
            .ToTable("AspNetUsers", t => t.ExcludeFromMigrations())
            .HasOne(x => x.UserProfile)
            .WithOne()
            .HasForeignKey<UserProfile>(x => x.UserId)
            .IsRequired();

        modelBuilder
            .Entity<Partner>(p =>
            {
                p.HasOne(x => x.PartnerOffice)
                    .WithMany()
                    .HasForeignKey(x => x.PartnerOfficeId);

                p.HasOne(x => x.PartnerCategory)
                    .WithMany()
                    .HasForeignKey(x => x.PartnerCategoryId);
            });

        modelBuilder
            .Entity<Contact>();
        
        modelBuilder.Entity<EntityUserRole>(entity =>
        {
            entity.HasOne(e => e.UserRole)
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.RoleId });
        });

        modelBuilder.Entity<Interaction>()
            .HasOne(i => i.Contact)
            .WithMany(c => c.Interactions)
            .HasForeignKey(i => i.ContactId);

        modelBuilder
            .Entity<PartnerTree>();

        modelBuilder
            .Entity<AiPrompt>();

        modelBuilder
            .Entity<AiScreenMapping>();

        modelBuilder
            .Entity<AiChatHistory>()
            .HasOne(a => a.Session)
            .WithMany(a => a.Chats)
            .HasForeignKey(a => a.SessionId);

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
            entity.Property(e => e.NameEmbedding)
              .HasColumnType("vector(768)");
            entity.HasIndex(e => new { e.EntityName, e.EntityId })
                    .IsUnique(); // This ensures uniqueness at the database level
        });
    }
}
