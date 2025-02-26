using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities.Common;
using Microsoft.Extensions.Hosting;
using UNOPS.PAO.Domain.Entities;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<UNOPSFundingOpportunity>()
            .HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectNumber)
            .HasPrincipalKey(x => x.ProjectNumber)
            .IsRequired(false);

        modelBuilder
            .Entity<UNOPSContact>();
            //.HasPrincipalKey(x => x.ContactNumber);
    }

    public new DbSet<UNOPSFundingOpportunity> FundingOpportunities { get; set; }
    public new DbSet<UNOPSDocument> Documents { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<WorkPackage> WorkPackages { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<BudgetLine> BudgetLines { get; set; }
    public DbSet<Donor> Donors { get; set; }
    public new DbSet<UNOPSContact> Contacts { get; set; }

    public new DbSet<UNOPSPartnerTree> PartnerTrees { get; set; }
}