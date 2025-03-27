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
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<Country> Countries { get; set; }

    public DbSet<EligibleEntity> EligibleEntities { get; set; }

    public DbSet<WorkflowLog> WorkflowLogs { get; set; }
    public DbSet<EntityUserRole> EntityUserRoles { get; set; }

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Interaction> Interactions { get; set; }
    public DbSet<Partner> Partners { get; set; }

    public DbSet<PartnerTree> PartnerTrees { get; set; }
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
    }
}
