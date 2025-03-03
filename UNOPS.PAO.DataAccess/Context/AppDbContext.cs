using UNOPS.PAO.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Domain.Entities;
using Microsoft.AspNetCore.Identity;

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
    public DbSet<FundingOpportunity> FundingOpportunities { get; set; }
    public DbSet<SelectionMethodology> SelectionMethodologies { get; set; }
    public DbSet<Proposal> Proposals { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentRelationship> DocumentRelationships { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<SDG> SDGs { get; set; }
    public DbSet<Country> Countries { get; set; }

    public DbSet<EligibleEntity> EligibleEntities { get; set; }

    public DbSet<WorkflowLog> WorkflowLogs { get; set; }
    public DbSet<EntityUserRole> EntityUserRoles { get; set; }

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Interaction> Interactions { get; set; }
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
            .Entity<FundingOpportunity>(opp =>
            {
                opp.HasOne(x => x.SelectionMethodology)
                    .WithMany()
                    .IsRequired(false);

                opp.HasMany(x => x.SDGs)
                    .WithMany()
                    .UsingEntity("FundingOpportunitySDGs");

                opp.HasMany(x => x.Countries)
                    .WithMany()
                    .UsingEntity("FundingOpportunityCountries");

                opp.HasOne(x => x.Currency)
                    .WithMany()
                    .IsRequired(false);

                opp.HasMany(x => x.EligibleEntities)
                    .WithMany()
                    .UsingEntity("FundingOpportunityEligibleEntities");
            });

        modelBuilder
            .Entity<Contact>();

        modelBuilder
            .Entity<Proposal>(proposal =>
            {
                proposal.HasOne(x => x.FundingOpportunity)
                    .WithMany(x => x.Proposals)
                    .HasForeignKey(x => x.FundingOpportunityId)
                    .IsRequired();

                proposal.HasOne(x => x.Applicant)
                    .WithMany()
                    .HasForeignKey(x => x.ApplicantId)
                    .IsRequired(false);
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
    }
}
